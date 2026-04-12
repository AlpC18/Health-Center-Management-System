using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Models.Domain;
using WellnessAPI.Services;

namespace WellnessAPI.Controllers;

// ── SHERBIMET ───────────────────────────────────────────────────────────────
/// <summary>
/// Kontrolluesi për menaxhimin e shërbimeve të qendrës wellness.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SherbimetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public SherbimetController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    /// <summary>
    /// Merr listën e të gjitha shërbimeve me mundësi kërkimi dhe paginimi.
    /// </summary>
    /// <param name="search">Teksti për kërkim (emri i shërbimit ose kategoria).</param>
    /// <param name="page">Numri i faqes.</param>
    /// <param name="limit">Numri i elementeve për faqe.</param>
    /// <returns>Një listë shërbimesh dhe informacione për paginim.</returns>
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.Sherbimet.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(search)) q = q.Where(s => s.EmriSherbimit.Contains(search!) || s.Kategoria!.Contains(search!));
        var total = await q.CountAsync();
        var data = await q.OrderBy(s => s.EmriSherbimit).Skip((page - 1) * limit).Take(limit)
            .Select(s => new SherbimResponseDto(s.SherbimId, s.EmriSherbimit, s.Kategoria, s.Pershkrimi, s.KohezgjatjaMin, s.Cmimi, s.Aktiv)).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var s = await _db.Sherbimet.FindAsync(id);
        if (s == null) return NotFound();
        return Ok(new SherbimResponseDto(s.SherbimId, s.EmriSherbimit, s.Kategoria, s.Pershkrimi, s.KohezgjatjaMin, s.Cmimi, s.Aktiv));
    }

    /// <summary>
    /// Krijon një shërbim të ri.
    /// </summary>
    /// <param name="dto">Të dhënat e shërbimit të ri.</param>
    /// <returns>Shërbimin e krijuar.</returns>
    [HttpPost]
    public async Task<ActionResult> Create(SherbimCreateDto dto)
    {
        var s = new Sherbim { EmriSherbimit = dto.EmriSherbimit, Kategoria = dto.Kategoria, Pershkrimi = dto.Pershkrimi, KohezgjatjaMin = dto.KohezgjatjaMin, Cmimi = dto.Cmimi, Aktiv = dto.Aktiv };
        _db.Sherbimet.Add(s);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Sherbim", s.SherbimId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = s.SherbimId }, s);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, SherbimUpdateDto dto)
    {
        var s = await _db.Sherbimet.FindAsync(id);
        if (s == null) return NotFound();
        var old = s;
        s.EmriSherbimit = dto.EmriSherbimit; s.Kategoria = dto.Kategoria; s.Pershkrimi = dto.Pershkrimi; s.KohezgjatjaMin = dto.KohezgjatjaMin; s.Cmimi = dto.Cmimi; s.Aktiv = dto.Aktiv;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Sherbim", id.ToString(), old, dto);
        return Ok(s);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var s = await _db.Sherbimet.FindAsync(id);
        if (s == null) return NotFound();
        _db.Sherbimet.Remove(s);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "Sherbim", id.ToString(), s, null);
        return NoContent();
    }
}

// ── TERAPISTET ──────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TerapistetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public TerapistetController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.Terapistet.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(search)) q = q.Where(t => t.Emri.Contains(search!) || t.Mbiemri.Contains(search!) || t.Email.Contains(search!));
        var total = await q.CountAsync();
        var data = await q.OrderBy(t => t.Mbiemri).Skip((page - 1) * limit).Take(limit)
            .Select(t => new TerapistResponseDto(t.TerapistId, t.Emri, t.Mbiemri, t.Specializimi, t.Licenca, t.Email, t.Telefoni, t.Aktiv)).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var t = await _db.Terapistet.FindAsync(id);
        if (t == null) return NotFound();
        return Ok(new TerapistResponseDto(t.TerapistId, t.Emri, t.Mbiemri, t.Specializimi, t.Licenca, t.Email, t.Telefoni, t.Aktiv));
    }

    [HttpPost]
    public async Task<ActionResult> Create(TerapistCreateDto dto)
    {
        var t = new Terapist { Emri = dto.Emri, Mbiemri = dto.Mbiemri, Specializimi = dto.Specializimi, Licenca = dto.Licenca, Email = dto.Email, Telefoni = dto.Telefoni, Aktiv = dto.Aktiv };
        _db.Terapistet.Add(t);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Terapist", t.TerapistId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = t.TerapistId }, t);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, TerapistUpdateDto dto)
    {
        var t = await _db.Terapistet.FindAsync(id);
        if (t == null) return NotFound();
        var old = t;
        t.Emri = dto.Emri; t.Mbiemri = dto.Mbiemri; t.Specializimi = dto.Specializimi; t.Licenca = dto.Licenca; t.Email = dto.Email; t.Telefoni = dto.Telefoni; t.Aktiv = dto.Aktiv;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Terapist", id.ToString(), old, dto);
        return Ok(t);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var t = await _db.Terapistet.FindAsync(id);
        if (t == null) return NotFound();
        _db.Terapistet.Remove(t);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "Terapist", id.ToString(), t, null);
        return NoContent();
    }
}

// ── TERMINET ────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TerminetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public TerminetController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.Terminet.Include(t => t.Klienti).Include(t => t.Sherbimi).Include(t => t.Terapisti).AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(t => t.Statusi == status);
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(t => t.DataTerminit).Skip((page - 1) * limit).Take(limit)
            .Select(t => new TerminResponseDto(t.TerminId, t.KlientId, t.Klienti.Emri + " " + t.Klienti.Mbiemri, t.SherbimId, t.Sherbimi.EmriSherbimit, t.TerapistId, t.Terapisti.Emri + " " + t.Terapisti.Mbiemri, t.DataTerminit, t.OraFillimit, t.OraMbarimit, t.Statusi, t.Shenimet)).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var t = await _db.Terminet.Include(x => x.Klienti).Include(x => x.Sherbimi).Include(x => x.Terapisti).FirstOrDefaultAsync(x => x.TerminId == id);
        if (t == null) return NotFound();
        return Ok(new TerminResponseDto(t.TerminId, t.KlientId, t.Klienti.Emri + " " + t.Klienti.Mbiemri, t.SherbimId, t.Sherbimi.EmriSherbimit, t.TerapistId, t.Terapisti.Emri + " " + t.Terapisti.Mbiemri, t.DataTerminit, t.OraFillimit, t.OraMbarimit, t.Statusi, t.Shenimet));
    }

    [HttpPost]
    public async Task<ActionResult> Create(TerminCreateDto dto)
    {
        // Conflict Detection: Check if therapist is busy
        var conflict = await _db.Terminet.AnyAsync(x => 
            x.TerapistId == dto.TerapistId && 
            x.DataTerminit.Date == dto.DataTerminit.Date &&
            ((dto.OraFillimit >= x.OraFillimit && dto.OraFillimit < x.OraMbarimit) || 
             (dto.OraMbarimit > x.OraFillimit && dto.OraMbarimit <= x.OraMbarimit) ||
             (dto.OraFillimit <= x.OraFillimit && dto.OraMbarimit >= x.OraMbarimit))
        );

        if (conflict)
            return Conflict(new { message = "Terapisti është i zënë në këtë orar." });

        var t = new Termin { KlientId = dto.KlientId, SherbimId = dto.SherbimId, TerapistId = dto.TerapistId, DataTerminit = dto.DataTerminit, OraFillimit = dto.OraFillimit, OraMbarimit = dto.OraMbarimit, Statusi = dto.Statusi ?? "Planifikuar", Shenimet = dto.Shenimet };
        _db.Terminet.Add(t);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Termin", t.TerminId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = t.TerminId }, t);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, TerminUpdateDto dto)
    {
        var t = await _db.Terminet.FindAsync(id);
        if (t == null) return NotFound();

        // Conflict Detection: Check if therapist is busy (excluding current record)
        var conflict = await _db.Terminet.AnyAsync(x => 
            x.TerminId != id &&
            x.TerapistId == dto.TerapistId && 
            x.DataTerminit.Date == dto.DataTerminit.Date &&
            ((dto.OraFillimit >= x.OraFillimit && dto.OraFillimit < x.OraMbarimit) || 
             (dto.OraMbarimit > x.OraFillimit && dto.OraMbarimit <= x.OraMbarimit) ||
             (dto.OraFillimit <= x.OraFillimit && dto.OraMbarimit >= x.OraMbarimit))
        );

        if (conflict)
            return Conflict(new { message = "Terapisti është i zënë në këtë orar." });

        var old = t;
        t.KlientId = dto.KlientId; t.SherbimId = dto.SherbimId; t.TerapistId = dto.TerapistId; t.DataTerminit = dto.DataTerminit; t.OraFillimit = dto.OraFillimit; t.OraMbarimit = dto.OraMbarimit; t.Statusi = dto.Statusi; t.Shenimet = dto.Shenimet;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Termin", id.ToString(), old, dto);
        return Ok(t);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var t = await _db.Terminet.FindAsync(id);
        if (t == null) return NotFound();
        _db.Terminet.Remove(t);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "Termin", id.ToString(), t, null);
        return NoContent();
    }
}

// ── PAKETA WELLNESS ─────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaketaWellnessController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public PaketaWellnessController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.PaketaWellness.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(search)) q = q.Where(p => p.EmriPaketes.Contains(search!));
        var total = await q.CountAsync();
        var data = await q.OrderBy(p => p.EmriPaketes).Skip((page - 1) * limit).Take(limit)
            .Select(p => new PaketaResponseDto(p.PaketId, p.EmriPaketes, p.Pershkrimi, p.SherbimiPerfshire, p.Cmimi, p.KohezgjatjaMuaj, p.Aktive)).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var p = await _db.PaketaWellness.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(new PaketaResponseDto(p.PaketId, p.EmriPaketes, p.Pershkrimi, p.SherbimiPerfshire, p.Cmimi, p.KohezgjatjaMuaj, p.Aktive));
    }

    [HttpPost]
    public async Task<ActionResult> Create(PaketaCreateDto dto)
    {
        var p = new PaketaWellness { EmriPaketes = dto.EmriPaketes, Pershkrimi = dto.Pershkrimi, SherbimiPerfshire = dto.SherbimiPerfshire, Cmimi = dto.Cmimi, KohezgjatjaMuaj = dto.KohezgjatjaMuaj, Aktive = dto.Aktive };
        _db.PaketaWellness.Add(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "PaketaWellness", p.PaketId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = p.PaketId }, p);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, PaketaUpdateDto dto)
    {
        var p = await _db.PaketaWellness.FindAsync(id);
        if (p == null) return NotFound();
        var old = p;
        p.EmriPaketes = dto.EmriPaketes; p.Pershkrimi = dto.Pershkrimi; p.SherbimiPerfshire = dto.SherbimiPerfshire; p.Cmimi = dto.Cmimi; p.KohezgjatjaMuaj = dto.KohezgjatjaMuaj; p.Aktive = dto.Aktive;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "PaketaWellness", id.ToString(), old, dto);
        return Ok(p);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var p = await _db.PaketaWellness.FindAsync(id);
        if (p == null) return NotFound();
        _db.PaketaWellness.Remove(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "PaketaWellness", id.ToString(), p, null);
        return NoContent();
    }
}

// ── ANETARESIMET ────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnetaresimetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public AnetaresimetController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.Anetaresimet.Include(a => a.Klienti).Include(a => a.Paketa).AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(a => a.Statusi == status);
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(a => a.DataFillimit).Skip((page - 1) * limit).Take(limit)
            .Select(a => new AnetaresimResponseDto(a.AnetaresimId, a.KlientId, a.Klienti.Emri + " " + a.Klienti.Mbiemri, a.PaketId, a.Paketa.EmriPaketes, a.DataFillimit, a.DataMbarimit, a.Statusi, a.CmimiPaguar)).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var a = await _db.Anetaresimet.Include(x => x.Klienti).Include(x => x.Paketa).FirstOrDefaultAsync(x => x.AnetaresimId == id);
        if (a == null) return NotFound();
        return Ok(new AnetaresimResponseDto(a.AnetaresimId, a.KlientId, a.Klienti.Emri + " " + a.Klienti.Mbiemri, a.PaketId, a.Paketa.EmriPaketes, a.DataFillimit, a.DataMbarimit, a.Statusi, a.CmimiPaguar));
    }

    [HttpPost]
    public async Task<ActionResult> Create(AnetaresimCreateDto dto)
    {
        var a = new Anetaresim { KlientId = dto.KlientId, PaketId = dto.PaketId, DataFillimit = dto.DataFillimit, DataMbarimit = dto.DataMbarimit, Statusi = dto.Statusi, CmimiPaguar = dto.CmimiPaguar };
        _db.Anetaresimet.Add(a);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Anetaresim", a.AnetaresimId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = a.AnetaresimId }, a);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, AnetaresimUpdateDto dto)
    {
        var a = await _db.Anetaresimet.FindAsync(id);
        if (a == null) return NotFound();
        var old = a;
        a.KlientId = dto.KlientId; a.PaketId = dto.PaketId; a.DataFillimit = dto.DataFillimit; a.DataMbarimit = dto.DataMbarimit; a.Statusi = dto.Statusi; a.CmimiPaguar = dto.CmimiPaguar;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Anetaresim", id.ToString(), old, dto);
        return Ok(a);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var a = await _db.Anetaresimet.FindAsync(id);
        if (a == null) return NotFound();
        _db.Anetaresimet.Remove(a);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "Anetaresim", id.ToString(), a, null);
        return NoContent();
    }
}

// ── PROGRAMET ───────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgrametController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public ProgrametController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.Programet.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(search)) q = q.Where(p => p.EmriProgramit.Contains(search!));
        var total = await q.CountAsync();
        var data = await q.OrderBy(p => p.EmriProgramit).Skip((page - 1) * limit).Take(limit)
            .Select(p => new ProgramResponseDto(p.ProgramId, p.EmriProgramit, p.Pershkrimi, p.KohezgjatjaJave, p.Qellimi, p.Ushtrimet, p.Dieta)).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var p = await _db.Programet.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(new ProgramResponseDto(p.ProgramId, p.EmriProgramit, p.Pershkrimi, p.KohezgjatjaJave, p.Qellimi, p.Ushtrimet, p.Dieta));
    }

    [HttpPost]
    public async Task<ActionResult> Create(ProgramCreateDto dto)
    {
        var p = new Models.Domain.Program { EmriProgramit = dto.EmriProgramit, Pershkrimi = dto.Pershkrimi, KohezgjatjaJave = dto.KohezgjatjaJave, Qellimi = dto.Qellimi, Ushtrimet = dto.Ushtrimet, Dieta = dto.Dieta };
        _db.Programet.Add(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Program", p.ProgramId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = p.ProgramId }, p);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, ProgramUpdateDto dto)
    {
        var p = await _db.Programet.FindAsync(id);
        if (p == null) return NotFound();
        var old = p;
        p.EmriProgramit = dto.EmriProgramit; p.Pershkrimi = dto.Pershkrimi; p.KohezgjatjaJave = dto.KohezgjatjaJave; p.Qellimi = dto.Qellimi; p.Ushtrimet = dto.Ushtrimet; p.Dieta = dto.Dieta;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Program", id.ToString(), old, dto);
        return Ok(p);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var p = await _db.Programet.FindAsync(id);
        if (p == null) return NotFound();
        _db.Programet.Remove(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "Program", id.ToString(), p, null);
        return NoContent();
    }
}

// ── PRODUKTET ───────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProduktetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public ProduktetController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.Produktet.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(search)) q = q.Where(p => p.EmriProduktit.Contains(search!) || p.Kategoria!.Contains(search!));
        var total = await q.CountAsync();
        var data = await q.OrderBy(p => p.EmriProduktit).Skip((page - 1) * limit).Take(limit)
            .Select(p => new ProduktResponseDto(p.ProduktId, p.EmriProduktit, p.Kategoria, p.Pershkrimi, p.Cmimi, p.SasiaStok, p.Aktiv)).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var p = await _db.Produktet.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(new ProduktResponseDto(p.ProduktId, p.EmriProduktit, p.Kategoria, p.Pershkrimi, p.Cmimi, p.SasiaStok, p.Aktiv));
    }

    [HttpPost]
    public async Task<ActionResult> Create(ProduktCreateDto dto)
    {
        var p = new Produkt { EmriProduktit = dto.EmriProduktit, Kategoria = dto.Kategoria, Pershkrimi = dto.Pershkrimi, Cmimi = dto.Cmimi, SasiaStok = dto.SasiaStok, Aktiv = dto.Aktiv };
        _db.Produktet.Add(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Produkt", p.ProduktId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = p.ProduktId }, p);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, ProduktUpdateDto dto)
    {
        var p = await _db.Produktet.FindAsync(id);
        if (p == null) return NotFound();
        var old = p;
        p.EmriProduktit = dto.EmriProduktit; p.Kategoria = dto.Kategoria; p.Pershkrimi = dto.Pershkrimi; p.Cmimi = dto.Cmimi; p.SasiaStok = dto.SasiaStok; p.Aktiv = dto.Aktiv;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Produkt", id.ToString(), old, dto);
        return Ok(p);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var p = await _db.Produktet.FindAsync(id);
        if (p == null) return NotFound();
        _db.Produktet.Remove(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "Produkt", id.ToString(), p, null);
        return NoContent();
    }
}

// ── SHITJET ─────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShitjetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public ShitjetController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.ShitjetProduktet.Include(s => s.Klienti).Include(s => s.Produkti).AsNoTracking().AsQueryable();
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(s => s.DataShitjes).Skip((page - 1) * limit).Take(limit)
            .Select(s => new ShitjeResponseDto(s.ShitjeId, s.KlientId, s.Klienti.Emri + " " + s.Klienti.Mbiemri, s.ProduktId, s.Produkti.EmriProduktit, s.Sasia, s.CmimiTotal, s.DataShitjes)).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var s = await _db.ShitjetProduktet.Include(x => x.Klienti).Include(x => x.Produkti).FirstOrDefaultAsync(x => x.ShitjeId == id);
        if (s == null) return NotFound();
        return Ok(new ShitjeResponseDto(s.ShitjeId, s.KlientId, s.Klienti.Emri + " " + s.Klienti.Mbiemri, s.ProduktId, s.Produkti.EmriProduktit, s.Sasia, s.CmimiTotal, s.DataShitjes));
    }

    [HttpPost]
    public async Task<ActionResult> Create(ShitjeCreateDto dto)
    {
        var s = new ShitjeProdukteve { KlientId = dto.KlientId, ProduktId = dto.ProduktId, Sasia = dto.Sasia, CmimiTotal = dto.CmimiTotal, DataShitjes = DateTime.UtcNow };
        _db.ShitjetProduktet.Add(s);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Shitje", s.ShitjeId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = s.ShitjeId }, s);
    }
}

// ── VLEREISIMET ─────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VlereisimetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public VlereisimetController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.Vlereisimet.Include(v => v.Klienti).Include(v => v.Sherbimi).Include(v => v.Terapisti).AsNoTracking().AsQueryable();
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(v => v.DataVleresimit).Skip((page - 1) * limit).Take(limit)
            .Select(v => new VleresimResponseDto(v.VleresimId, v.KlientId, v.Klienti.Emri + " " + v.Klienti.Mbiemri, v.SherbimId, v.Sherbimi.EmriSherbimit, v.TerapistId, v.Terapisti.Emri + " " + v.Terapisti.Mbiemri, v.Nota, v.Komenti, v.DataVleresimit)).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var v = await _db.Vlereisimet.Include(x => x.Klienti).Include(x => x.Sherbimi).Include(x => x.Terapisti).FirstOrDefaultAsync(x => x.VleresimId == id);
        if (v == null) return NotFound();
        return Ok(new VleresimResponseDto(v.VleresimId, v.KlientId, v.Klienti.Emri + " " + v.Klienti.Mbiemri, v.SherbimId, v.Sherbimi.EmriSherbimit, v.TerapistId, v.Terapisti.Emri + " " + v.Terapisti.Mbiemri, v.Nota, v.Komenti, v.DataVleresimit));
    }

    [HttpPost]
    public async Task<ActionResult> Create(VleresimCreateDto dto)
    {
        var v = new Vleresim { KlientId = dto.KlientId, SherbimId = dto.SherbimId, TerapistId = dto.TerapistId, Nota = dto.Nota, Komenti = dto.Komenti, DataVleresimit = DateTime.UtcNow };
        _db.Vlereisimet.Add(v);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Vleresim", v.VleresimId.ToString(), null, dto);
        return CreatedAtAction(nameof(GetById), new { id = v.VleresimId }, v);
    }
}

// ── KLIENT PROGRAMET ────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KlientProgrametController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public KlientProgrametController(ApplicationDbContext db) { _db = db; }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var q = _db.KlientProgramet.Include(kp => kp.Klienti).Include(kp => kp.Programi).AsNoTracking().AsQueryable();
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(kp => kp.DataFillimit).Skip((page - 1) * limit).Take(limit)
            .Select(kp => new { kp.KpId, kp.KlientId, KlientEmri = kp.Klienti.Emri + " " + kp.Klienti.Mbiemri, kp.ProgramId, ProgramEmri = kp.Programi.EmriProgramit, kp.DataFillimit, kp.DataMbarimit, kp.Progresi, kp.Statusi }).ToListAsync();
        return Ok(new { data, total, page, limit });
    }
}
