using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.Models.Domain;
using WellnessAPI.Services;

namespace WellnessAPI.Controllers;

// ════════════════════════════════════════════════════════════════════════════
// 5 ADDITIONAL CRUD ENDPOINTS
// ════════════════════════════════════════════════════════════════════════════
// Each controller exposes the standard 5 actions:
//   GET    /api/{entity}          — list (with search + pagination)
//   GET    /api/{entity}/{id}     — single record
//   POST   /api/{entity}          — create
//   PUT    /api/{entity}/{id}     — update
//   DELETE /api/{entity}/{id}     — delete
// All endpoints require an authenticated user and write to AuditLog.
// ════════════════════════════════════════════════════════════════════════════

// ── 1. SALLAT (Therapy / treatment rooms) ───────────────────────────────────

public record SallaCreateDto(string Emri, int Kapaciteti, string? Tipi, string? Pershkrimi, bool Aktive);
public record SallaUpdateDto(string Emri, int Kapaciteti, string? Tipi, string? Pershkrimi, bool Aktive);

/// <summary>Kontrolluesi për menaxhimin e sallave të trajtimit.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SallatController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public SallatController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? aktive, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.Sallat.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(search)) q = q.Where(s => s.Emri.Contains(search!) || (s.Tipi != null && s.Tipi.Contains(search!)));
        if (aktive.HasValue) q = q.Where(s => s.Aktive == aktive.Value);
        var total = await q.CountAsync();
        var data = await q.OrderBy(s => s.Emri).Skip((page - 1) * limit).Take(limit).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var s = await _db.Sallat.FindAsync(id);
        if (s == null) return NotFound();
        return Ok(s);
    }

    [HttpPost]
    public async Task<ActionResult> Create(SallaCreateDto dto)
    {
        var s = new Salla { Emri = dto.Emri, Kapaciteti = dto.Kapaciteti, Tipi = dto.Tipi, Pershkrimi = dto.Pershkrimi, Aktive = dto.Aktive };
        _db.Sallat.Add(s);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Salla", s.SallaId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = s.SallaId }, s);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, SallaUpdateDto dto)
    {
        var s = await _db.Sallat.FindAsync(id);
        if (s == null) return NotFound();
        var old = new { s.Emri, s.Kapaciteti, s.Tipi, s.Pershkrimi, s.Aktive };
        s.Emri = dto.Emri; s.Kapaciteti = dto.Kapaciteti; s.Tipi = dto.Tipi; s.Pershkrimi = dto.Pershkrimi; s.Aktive = dto.Aktive;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Salla", id.ToString(), old, dto);
        return Ok(s);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var s = await _db.Sallat.FindAsync(id);
        if (s == null) return NotFound();
        _db.Sallat.Remove(s);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "Salla", id.ToString(), s, null);
        return NoContent();
    }
}

// ── 2. FURNIZUESIT (Suppliers) ──────────────────────────────────────────────

public record FurnizuesiCreateDto(string Emri, string? KontaktPersona, string? Email, string? Telefoni, string? Adresa, bool Aktiv);
public record FurnizuesiUpdateDto(string Emri, string? KontaktPersona, string? Email, string? Telefoni, string? Adresa, bool Aktiv);

/// <summary>Kontrolluesi për menaxhimin e furnizuesve të produkteve.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FurnizuesitController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public FurnizuesitController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? aktiv, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.Furnizuesit.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(search)) q = q.Where(f => f.Emri.Contains(search!) || (f.Email != null && f.Email.Contains(search!)));
        if (aktiv.HasValue) q = q.Where(f => f.Aktiv == aktiv.Value);
        var total = await q.CountAsync();
        var data = await q.OrderBy(f => f.Emri).Skip((page - 1) * limit).Take(limit).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var f = await _db.Furnizuesit.FindAsync(id);
        if (f == null) return NotFound();
        return Ok(f);
    }

    [HttpPost]
    public async Task<ActionResult> Create(FurnizuesiCreateDto dto)
    {
        var f = new Furnizuesi { Emri = dto.Emri, KontaktPersona = dto.KontaktPersona, Email = dto.Email, Telefoni = dto.Telefoni, Adresa = dto.Adresa, Aktiv = dto.Aktiv };
        _db.Furnizuesit.Add(f);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Furnizuesi", f.FurnizuesId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = f.FurnizuesId }, f);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, FurnizuesiUpdateDto dto)
    {
        var f = await _db.Furnizuesit.FindAsync(id);
        if (f == null) return NotFound();
        var old = new { f.Emri, f.KontaktPersona, f.Email, f.Telefoni, f.Adresa, f.Aktiv };
        f.Emri = dto.Emri; f.KontaktPersona = dto.KontaktPersona; f.Email = dto.Email; f.Telefoni = dto.Telefoni; f.Adresa = dto.Adresa; f.Aktiv = dto.Aktiv;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Furnizuesi", id.ToString(), old, dto);
        return Ok(f);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var f = await _db.Furnizuesit.FindAsync(id);
        if (f == null) return NotFound();
        _db.Furnizuesit.Remove(f);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "Furnizuesi", id.ToString(), f, null);
        return NoContent();
    }
}

// ── 3. LAJMERIMET (Announcements) ───────────────────────────────────────────

public record LajmerimiCreateDto(string Titulli, string Permbajtja, string? Audienca, string? Prioriteti, DateTime? DataSkadimit, bool Aktiv);
public record LajmerimiUpdateDto(string Titulli, string Permbajtja, string? Audienca, string? Prioriteti, DateTime? DataSkadimit, bool Aktiv);

/// <summary>Kontrolluesi për menaxhimin e lajmërimeve të sistemit.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LajmerimetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public LajmerimetController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] string? search, [FromQuery] string? audienca, [FromQuery] bool? aktiv, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.Lajmerimet.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(search)) q = q.Where(l => l.Titulli.Contains(search!) || l.Permbajtja.Contains(search!));
        if (!string.IsNullOrEmpty(audienca)) q = q.Where(l => l.Audienca == audienca);
        if (aktiv.HasValue) q = q.Where(l => l.Aktiv == aktiv.Value);
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(l => l.DataKrijimit).Skip((page - 1) * limit).Take(limit).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var l = await _db.Lajmerimet.FindAsync(id);
        if (l == null) return NotFound();
        return Ok(l);
    }

    [HttpPost]
    public async Task<ActionResult> Create(LajmerimiCreateDto dto)
    {
        var l = new Lajmerimi {
            Titulli = dto.Titulli,
            Permbajtja = dto.Permbajtja,
            Audienca = dto.Audienca ?? "All",
            Prioriteti = dto.Prioriteti ?? "Mesem",
            DataSkadimit = dto.DataSkadimit,
            Aktiv = dto.Aktiv,
            DataKrijimit = DateTime.UtcNow
        };
        _db.Lajmerimet.Add(l);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Lajmerimi", l.LajmerimId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = l.LajmerimId }, l);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, LajmerimiUpdateDto dto)
    {
        var l = await _db.Lajmerimet.FindAsync(id);
        if (l == null) return NotFound();
        var old = new { l.Titulli, l.Permbajtja, l.Audienca, l.Prioriteti, l.DataSkadimit, l.Aktiv };
        l.Titulli = dto.Titulli;
        l.Permbajtja = dto.Permbajtja;
        l.Audienca = dto.Audienca ?? l.Audienca;
        l.Prioriteti = dto.Prioriteti ?? l.Prioriteti;
        l.DataSkadimit = dto.DataSkadimit;
        l.Aktiv = dto.Aktiv;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Lajmerimi", id.ToString(), old, dto);
        return Ok(l);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var l = await _db.Lajmerimet.FindAsync(id);
        if (l == null) return NotFound();
        _db.Lajmerimet.Remove(l);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "Lajmerimi", id.ToString(), l, null);
        return NoContent();
    }
}

// ── 4. ZBRITJET (Discount codes / coupons) ──────────────────────────────────

public record ZbritjeCreateDto(string Kodi, decimal PerqindjaZbritjes, DateTime DataFillimit, DateTime DataMbarimit, int LimitiPerdorimit, bool Aktive);
public record ZbritjeUpdateDto(string Kodi, decimal PerqindjaZbritjes, DateTime DataFillimit, DateTime DataMbarimit, int LimitiPerdorimit, bool Aktive);

/// <summary>Kontrolluesi për menaxhimin e kodeve të zbritjes.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ZbritjetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public ZbritjetController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? aktive, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.Zbritjet.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(search)) q = q.Where(z => z.Kodi.Contains(search!));
        if (aktive.HasValue) q = q.Where(z => z.Aktive == aktive.Value);
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(z => z.DataFillimit).Skip((page - 1) * limit).Take(limit).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var z = await _db.Zbritjet.FindAsync(id);
        if (z == null) return NotFound();
        return Ok(z);
    }

    [HttpPost]
    public async Task<ActionResult> Create(ZbritjeCreateDto dto)
    {
        if (dto.PerqindjaZbritjes < 0 || dto.PerqindjaZbritjes > 100)
            return BadRequest(new { message = "Përqindja e zbritjes duhet të jetë midis 0 dhe 100." });
        if (dto.DataMbarimit <= dto.DataFillimit)
            return BadRequest(new { message = "Data e mbarimit duhet të jetë pas datës së fillimit." });

        var z = new Zbritja {
            Kodi = dto.Kodi.ToUpperInvariant(),
            PerqindjaZbritjes = dto.PerqindjaZbritjes,
            DataFillimit = dto.DataFillimit,
            DataMbarimit = dto.DataMbarimit,
            LimitiPerdorimit = dto.LimitiPerdorimit,
            Aktive = dto.Aktive
        };
        _db.Zbritjet.Add(z);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Zbritja", z.ZbritjeId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = z.ZbritjeId }, z);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, ZbritjeUpdateDto dto)
    {
        var z = await _db.Zbritjet.FindAsync(id);
        if (z == null) return NotFound();
        if (dto.PerqindjaZbritjes < 0 || dto.PerqindjaZbritjes > 100)
            return BadRequest(new { message = "Përqindja e zbritjes duhet të jetë midis 0 dhe 100." });
        if (dto.DataMbarimit <= dto.DataFillimit)
            return BadRequest(new { message = "Data e mbarimit duhet të jetë pas datës së fillimit." });

        var old = new { z.Kodi, z.PerqindjaZbritjes, z.DataFillimit, z.DataMbarimit, z.LimitiPerdorimit, z.Aktive };
        z.Kodi = dto.Kodi.ToUpperInvariant();
        z.PerqindjaZbritjes = dto.PerqindjaZbritjes;
        z.DataFillimit = dto.DataFillimit;
        z.DataMbarimit = dto.DataMbarimit;
        z.LimitiPerdorimit = dto.LimitiPerdorimit;
        z.Aktive = dto.Aktive;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Zbritja", id.ToString(), old, dto);
        return Ok(z);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var z = await _db.Zbritjet.FindAsync(id);
        if (z == null) return NotFound();
        _db.Zbritjet.Remove(z);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "Zbritja", id.ToString(), z, null);
        return NoContent();
    }
}

// ── 5. PUSHIMET (Therapist time-off requests) ───────────────────────────────

public record PushimiCreateDto(int TerapistId, DateTime DataFillimit, DateTime DataMbarimit, string? Arsyeja, string? Statusi);
public record PushimiUpdateDto(int TerapistId, DateTime DataFillimit, DateTime DataMbarimit, string? Arsyeja, string Statusi);

/// <summary>Kontrolluesi për menaxhimin e kërkesave për pushim të terapistëve.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PushimetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public PushimetController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int? terapistId, [FromQuery] string? statusi, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.Pushimet.AsNoTracking().AsQueryable();
        if (terapistId.HasValue) q = q.Where(p => p.TerapistId == terapistId.Value);
        if (!string.IsNullOrEmpty(statusi)) q = q.Where(p => p.Statusi == statusi);
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(p => p.DataFillimit).Skip((page - 1) * limit).Take(limit).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var p = await _db.Pushimet.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(p);
    }

    [HttpPost]
    public async Task<ActionResult> Create(PushimiCreateDto dto)
    {
        if (dto.DataMbarimit <= dto.DataFillimit)
            return BadRequest(new { message = "Data e mbarimit duhet të jetë pas datës së fillimit." });
        var terapistExists = await _db.Terapistet.AnyAsync(t => t.TerapistId == dto.TerapistId);
        if (!terapistExists)
            return BadRequest(new { message = "Terapisti i zgjedhur nuk ekziston." });

        var p = new Pushimi {
            TerapistId = dto.TerapistId,
            DataFillimit = dto.DataFillimit,
            DataMbarimit = dto.DataMbarimit,
            Arsyeja = dto.Arsyeja,
            Statusi = dto.Statusi ?? "Kerkuar",
            DataKerkimit = DateTime.UtcNow
        };
        _db.Pushimet.Add(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Pushimi", p.PushimId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = p.PushimId }, p);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, PushimiUpdateDto dto)
    {
        var p = await _db.Pushimet.FindAsync(id);
        if (p == null) return NotFound();
        if (dto.DataMbarimit <= dto.DataFillimit)
            return BadRequest(new { message = "Data e mbarimit duhet të jetë pas datës së fillimit." });

        var old = new { p.TerapistId, p.DataFillimit, p.DataMbarimit, p.Arsyeja, p.Statusi };
        p.TerapistId = dto.TerapistId;
        p.DataFillimit = dto.DataFillimit;
        p.DataMbarimit = dto.DataMbarimit;
        p.Arsyeja = dto.Arsyeja;
        p.Statusi = dto.Statusi;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Pushimi", id.ToString(), old, dto);
        return Ok(p);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var p = await _db.Pushimet.FindAsync(id);
        if (p == null) return NotFound();
        _db.Pushimet.Remove(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "Pushimi", id.ToString(), p, null);
        return NoContent();
    }
}
