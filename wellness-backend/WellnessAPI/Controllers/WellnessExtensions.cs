// Controllers for Phase-2 wellness features:
//   - Per-visit clinical notes  (KlientShenime)
//   - Body measurements          (KlientMatjet)
//   - Loyalty points ledger      (KlientPikat)
//   - Therapist self-service     (TherapistPortal)
//   - Recurring bookings         (extension to Terminet)
//
// Role conventions:
//   - Admin             : full access, all rows.
//   - Therapist         : sees only data for their own appointments / clients.
//   - Klient            : reads-only their own clinical notes (non-private) and measurements/points.
//
// All write endpoints log via AuditService for auditability.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Models.Domain;
using WellnessAPI.Models.Identity;
using WellnessAPI.Services;

namespace WellnessAPI.Controllers;

// ── Shared helpers ──────────────────────────────────────────────────────────
internal static class RoleScoping
{
    /// <summary>Read the Identity UserId out of the JWT claims.</summary>
    public static string? UserId(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>Returns the TerapistId linked to the current user, or null.</summary>
    public static async Task<int?> CurrentTerapistIdAsync(this ClaimsPrincipal principal, UserManager<ApplicationUser> userManager)
    {
        var uid = principal.UserId();
        if (string.IsNullOrEmpty(uid)) return null;
        var user = await userManager.FindByIdAsync(uid);
        return int.TryParse(user?.TerapistId, out var id) ? id : null;
    }

    /// <summary>Returns the KlientId linked to the current user, or null.</summary>
    public static async Task<int?> CurrentKlientIdAsync(this ClaimsPrincipal principal, UserManager<ApplicationUser> userManager)
    {
        var uid = principal.UserId();
        if (string.IsNullOrEmpty(uid)) return null;
        var user = await userManager.FindByIdAsync(uid);
        return int.TryParse(user?.KlientId, out var id) ? id : null;
    }
}

// ── 1. CLINICAL NOTES ───────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KlientShenimeController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    private readonly UserManager<ApplicationUser> _users;
    public KlientShenimeController(ApplicationDbContext db, AuditService audit, UserManager<ApplicationUser> users)
    { _db = db; _audit = audit; _users = users; }

    /// <summary>List notes. Admin sees all. Therapist sees their own. Klient sees only non-private notes about themselves.</summary>
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int? klientId, [FromQuery] int? terminId, [FromQuery] int page = 1, [FromQuery] int limit = 25)
    {
        var q = _db.KlientShenime.AsNoTracking().AsQueryable();

        if (User.IsInRole("Therapist") && !User.IsInRole("Admin"))
        {
            var tid = await User.CurrentTerapistIdAsync(_users);
            if (tid is null) return Forbid();
            q = q.Where(s => s.TerapistId == tid);
        }
        else if (User.IsInRole("Klient") && !User.IsInRole("Admin"))
        {
            var kid = await User.CurrentKlientIdAsync(_users);
            if (kid is null) return Forbid();
            q = q.Where(s => s.KlientId == kid && !s.Privat);
        }

        if (klientId.HasValue) q = q.Where(s => s.KlientId == klientId.Value);
        if (terminId.HasValue) q = q.Where(s => s.TerminId == terminId.Value);

        var total = await q.CountAsync();
        var rows = await q.OrderByDescending(s => s.DataKrijimit)
            .Skip((page - 1) * limit).Take(limit)
            .Join(_db.Klientet.AsNoTracking(), s => s.KlientId, k => k.KlientId, (s, k) => new { s, k })
            .GroupJoin(_db.Terapistet.AsNoTracking(), x => x.s.TerapistId, t => (int?)t.TerapistId, (x, ts) => new { x.s, x.k, ts })
            .SelectMany(x => x.ts.DefaultIfEmpty(), (x, t) => new KlientShenimResponseDto(
                x.s.ShenimId, x.s.KlientId, x.k.Emri + " " + x.k.Mbiemri,
                x.s.TerminId, x.s.TerapistId,
                t != null ? t.Emri + " " + t.Mbiemri : null,
                x.s.Tipi, x.s.Permbajtja, x.s.Privat, x.s.DataKrijimit))
            .ToListAsync();

        return Ok(new { data = rows, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var note = await _db.KlientShenime.AsNoTracking().FirstOrDefaultAsync(s => s.ShenimId == id);
        if (note is null) return NotFound();
        // Authorization: same rules as list
        if (User.IsInRole("Klient") && !User.IsInRole("Admin"))
        {
            var kid = await User.CurrentKlientIdAsync(_users);
            if (kid != note.KlientId || note.Privat) return Forbid();
        }
        else if (User.IsInRole("Therapist") && !User.IsInRole("Admin"))
        {
            var tid = await User.CurrentTerapistIdAsync(_users);
            if (note.TerapistId != tid) return Forbid();
        }
        return Ok(note);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Therapist")]
    public async Task<ActionResult> Create(KlientShenimCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Permbajtja))
            return BadRequest(new { message = "Përmbajtja nuk mund të jetë bosh." });

        var note = new KlientShenim
        {
            KlientId = dto.KlientId,
            TerminId = dto.TerminId,
            TerapistId = dto.TerapistId ?? await User.CurrentTerapistIdAsync(_users),
            Tipi = dto.Tipi,
            Permbajtja = dto.Permbajtja,
            Privat = dto.Privat,
            DataKrijimit = DateTime.UtcNow,
        };
        _db.KlientShenime.Add(note);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "KlientShenim", note.ShenimId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = note.ShenimId }, note);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Therapist")]
    public async Task<ActionResult> Update(int id, KlientShenimUpdateDto dto)
    {
        var note = await _db.KlientShenime.FindAsync(id);
        if (note is null) return NotFound();
        // Therapist can update only own notes
        if (User.IsInRole("Therapist") && !User.IsInRole("Admin"))
        {
            var tid = await User.CurrentTerapistIdAsync(_users);
            if (note.TerapistId != tid) return Forbid();
        }
        note.Tipi = dto.Tipi;
        note.Permbajtja = dto.Permbajtja;
        note.Privat = dto.Privat;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "KlientShenim", id.ToString(), null, dto);
        return Ok(note);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var note = await _db.KlientShenime.FindAsync(id);
        if (note is null) return NotFound();
        _db.KlientShenime.Remove(note);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "KlientShenim", id.ToString(), note, null);
        return Ok(new { success = true });
    }
}

// ── 2. BODY MEASUREMENTS ────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KlientMatjetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    private readonly UserManager<ApplicationUser> _users;
    public KlientMatjetController(ApplicationDbContext db, AuditService audit, UserManager<ApplicationUser> users)
    { _db = db; _audit = audit; _users = users; }

    /// <summary>Computes BMI from height + weight if both present. Pure helper, no side effects.</summary>
    private static decimal? Bmi(decimal? kg, decimal? cm)
    {
        if (!kg.HasValue || !cm.HasValue || cm.Value <= 0) return null;
        var m = cm.Value / 100m;
        return Math.Round(kg.Value / (m * m), 2);
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int? klientId, [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        var q = _db.KlientMatjet.AsNoTracking().AsQueryable();

        if (User.IsInRole("Klient") && !User.IsInRole("Admin") && !User.IsInRole("Therapist"))
        {
            var kid = await User.CurrentKlientIdAsync(_users);
            if (kid is null) return Forbid();
            q = q.Where(m => m.KlientId == kid);
        }
        if (klientId.HasValue) q = q.Where(m => m.KlientId == klientId.Value);

        var total = await q.CountAsync();
        var rows = await q.OrderByDescending(m => m.DataMatjes)
            .Skip((page - 1) * limit).Take(limit)
            .ToListAsync();

        var dtos = rows.Select(m => new KlientMatjeResponseDto(
            m.MatjeId, m.KlientId, m.DataMatjes,
            m.PeshaKg, m.GjatesiaCm, m.YndyraTrupore, m.BeliCm, m.KofshaCm,
            Bmi(m.PeshaKg, m.GjatesiaCm), m.Shenim)).ToList();

        return Ok(new { data = dtos, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var m = await _db.KlientMatjet.AsNoTracking().FirstOrDefaultAsync(x => x.MatjeId == id);
        if (m is null) return NotFound();
        if (User.IsInRole("Klient") && !User.IsInRole("Admin") && !User.IsInRole("Therapist"))
        {
            var kid = await User.CurrentKlientIdAsync(_users);
            if (kid != m.KlientId) return Forbid();
        }
        return Ok(new KlientMatjeResponseDto(m.MatjeId, m.KlientId, m.DataMatjes,
            m.PeshaKg, m.GjatesiaCm, m.YndyraTrupore, m.BeliCm, m.KofshaCm,
            Bmi(m.PeshaKg, m.GjatesiaCm), m.Shenim));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Therapist")]
    public async Task<ActionResult> Create(KlientMatjeCreateDto dto)
    {
        var m = new KlientMatje
        {
            KlientId = dto.KlientId,
            DataMatjes = dto.DataMatjes ?? DateTime.UtcNow,
            PeshaKg = dto.PeshaKg,
            GjatesiaCm = dto.GjatesiaCm,
            YndyraTrupore = dto.YndyraTrupore,
            BeliCm = dto.BeliCm,
            KofshaCm = dto.KofshaCm,
            Shenim = dto.Shenim,
        };
        _db.KlientMatjet.Add(m);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "KlientMatje", m.MatjeId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = m.MatjeId }, m);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Therapist")]
    public async Task<ActionResult> Update(int id, KlientMatjeUpdateDto dto)
    {
        var m = await _db.KlientMatjet.FindAsync(id);
        if (m is null) return NotFound();
        m.DataMatjes = dto.DataMatjes;
        m.PeshaKg = dto.PeshaKg;
        m.GjatesiaCm = dto.GjatesiaCm;
        m.YndyraTrupore = dto.YndyraTrupore;
        m.BeliCm = dto.BeliCm;
        m.KofshaCm = dto.KofshaCm;
        m.Shenim = dto.Shenim;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "KlientMatje", id.ToString(), null, dto);
        return Ok(m);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var m = await _db.KlientMatjet.FindAsync(id);
        if (m is null) return NotFound();
        _db.KlientMatjet.Remove(m);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "KlientMatje", id.ToString(), m, null);
        return Ok(new { success = true });
    }
}

// ── 3. LOYALTY POINTS ───────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KlientPikatController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    private readonly UserManager<ApplicationUser> _users;
    public KlientPikatController(ApplicationDbContext db, AuditService audit, UserManager<ApplicationUser> users)
    { _db = db; _audit = audit; _users = users; }

    /// <summary>Ledger entries for a client (or current klient if no id given).</summary>
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int? klientId, [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        if (User.IsInRole("Klient") && !User.IsInRole("Admin"))
        {
            var kid = await User.CurrentKlientIdAsync(_users);
            if (kid is null) return Forbid();
            klientId = kid;
        }

        var q = _db.KlientPikat.AsNoTracking().AsQueryable();
        if (klientId.HasValue) q = q.Where(p => p.KlientId == klientId.Value);

        var total = await q.CountAsync();
        var rows = await q.OrderByDescending(p => p.DataKrijimit)
            .Skip((page - 1) * limit).Take(limit)
            .Select(p => new KlientPikaResponseDto(p.PikaId, p.KlientId, p.Pike, p.Tipi, p.LidhjeId, p.Shenim, p.DataKrijimit))
            .ToListAsync();

        return Ok(new { data = rows, total, page, limit });
    }

    /// <summary>Current balance + lifetime earn/redeem totals.</summary>
    [HttpGet("balance/{klientId}")]
    public async Task<ActionResult> Balance(int klientId)
    {
        if (User.IsInRole("Klient") && !User.IsInRole("Admin"))
        {
            var kid = await User.CurrentKlientIdAsync(_users);
            if (kid != klientId) return Forbid();
        }
        var k = await _db.Klientet.AsNoTracking().FirstOrDefaultAsync(x => x.KlientId == klientId);
        if (k is null) return NotFound();
        var entries = await _db.KlientPikat.AsNoTracking().Where(p => p.KlientId == klientId).ToListAsync();
        var balance = entries.Sum(p => p.Pike);
        var earned = entries.Where(p => p.Pike > 0).Sum(p => p.Pike);
        var spent = -entries.Where(p => p.Pike < 0).Sum(p => p.Pike);
        return Ok(new KlientPikatBalanceDto(klientId, k.Emri + " " + k.Mbiemri, balance, earned, spent));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Create(KlientPikaCreateDto dto)
    {
        var p = new KlientPika
        {
            KlientId = dto.KlientId,
            Pike = dto.Pike,
            Tipi = dto.Tipi,
            LidhjeId = dto.LidhjeId,
            Shenim = dto.Shenim,
            DataKrijimit = DateTime.UtcNow,
        };
        _db.KlientPikat.Add(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "KlientPika", p.PikaId.ToString(), null, dto);
        return CreatedAtAction(nameof(Balance), new { klientId = p.KlientId }, p);
    }
}

// ── 4. THERAPIST PORTAL ─────────────────────────────────────────────────────
[ApiController]
[Route("api/terapist-portal")]
[Authorize(Roles = "Admin,Therapist")]
public class TherapistPortalController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    private readonly UserManager<ApplicationUser> _users;
    public TherapistPortalController(ApplicationDbContext db, AuditService audit, UserManager<ApplicationUser> users)
    { _db = db; _audit = audit; _users = users; }

    /// <summary>Profile of the currently-logged-in therapist.</summary>
    [HttpGet("me")]
    public async Task<ActionResult> Me()
    {
        var tid = await User.CurrentTerapistIdAsync(_users);
        if (tid is null) return Forbid();
        var t = await _db.Terapistet.AsNoTracking().FirstOrDefaultAsync(x => x.TerapistId == tid);
        if (t is null) return NotFound();
        return Ok(new {
            t.TerapistId, t.Emri, t.Mbiemri, t.Specializimi, t.Licenca,
            t.Email, t.Telefoni, t.Aktiv,
        });
    }

    /// <summary>Today + upcoming appointments for the current therapist.</summary>
    [HttpGet("my-schedule")]
    public async Task<ActionResult> MySchedule([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var tid = await User.CurrentTerapistIdAsync(_users);
        if (tid is null) return Forbid();

        var fromD = from?.Date ?? DateTime.UtcNow.Date;
        var toD = to?.Date.AddDays(1) ?? fromD.AddDays(14);

        var rows = await _db.Terminet.AsNoTracking()
            .Where(x => x.TerapistId == tid && x.DataTerminit >= fromD && x.DataTerminit < toD)
            .Include(x => x.Klienti).Include(x => x.Sherbimi)
            .OrderBy(x => x.DataTerminit).ThenBy(x => x.OraFillimit)
            .Select(x => new {
                x.TerminId, x.DataTerminit, x.OraFillimit, x.OraMbarimit, x.Statusi,
                klientId = x.KlientId, klientEmri = x.Klienti.Emri + " " + x.Klienti.Mbiemri,
                sherbimi = x.Sherbimi.EmriSherbimit,
                x.Shenimet
            })
            .ToListAsync();

        return Ok(rows);
    }

    /// <summary>Distinct clients the current therapist has seen.</summary>
    [HttpGet("my-clients")]
    public async Task<ActionResult> MyClients()
    {
        var tid = await User.CurrentTerapistIdAsync(_users);
        if (tid is null) return Forbid();

        var rows = await _db.Terminet.AsNoTracking()
            .Where(t => t.TerapistId == tid)
            .Select(t => t.KlientId)
            .Distinct()
            .Join(_db.Klientet.AsNoTracking(), id => id, k => k.KlientId, (id, k) => new {
                k.KlientId, k.Emri, k.Mbiemri, k.Email, k.Telefoni, k.DataLindjes
            })
            .OrderBy(k => k.Emri)
            .ToListAsync();

        return Ok(rows);
    }

    /// <summary>Allowed status transitions are enforced server-side: only Konfirmuar -> Perfunduar (or via the standard TerminetController).</summary>
    [HttpPost("appointments/{terminId}/complete")]
    public async Task<ActionResult> CompleteAppointment(int terminId)
    {
        var tid = await User.CurrentTerapistIdAsync(_users);
        if (tid is null) return Forbid();
        var t = await _db.Terminet.FindAsync(terminId);
        if (t is null) return NotFound();
        if (t.TerapistId != tid && !User.IsInRole("Admin")) return Forbid();
        if (t.Statusi != "Konfirmuar")
            return BadRequest(new { message = $"Termini duhet të jetë 'Konfirmuar' për t'u përfunduar (aktualisht: {t.Statusi})." });

        t.Statusi = "Perfunduar";
        await _db.SaveChangesAsync();
        await _audit.LogAsync("COMPLETE", "Termin", terminId.ToString(), null, new { newStatus = "Perfunduar" });

        // Award 10 loyalty points to the client for a completed appointment
        _db.KlientPikat.Add(new KlientPika
        {
            KlientId = t.KlientId,
            Pike = 10,
            Tipi = "Termin",
            LidhjeId = t.TerminId,
            Shenim = $"Termin i përfunduar #{t.TerminId}",
            DataKrijimit = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        return Ok(new { success = true, statusi = t.Statusi, message = "Termini u përfundua. Klienti fitoi 10 pikë." });
    }
}
