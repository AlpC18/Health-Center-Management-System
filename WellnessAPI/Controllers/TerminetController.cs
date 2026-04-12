using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Services;
using WellnessAPI.Helpers;
using WellnessAPI.Models.Domain;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/terminet")]
[Authorize]
public class TerminetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public TerminetController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query)
    {
        var q = _db.Terminet
            .AsNoTracking()
            .Include(t => t.Klienti)
            .Include(t => t.Sherbimi)
            .Include(t => t.Terapisti)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(t =>
                t.Statusi.ToLower().Contains(s) ||
                (t.Klienti != null && (t.Klienti.Emri + " " + t.Klienti.Mbiemri).ToLower().Contains(s)) ||
                (t.Sherbimi != null && t.Sherbimi.EmriSherbimit.ToLower().Contains(s)) ||
                (t.Terapisti != null && (t.Terapisti.Emri + " " + t.Terapisti.Mbiemri).ToLower().Contains(s)));
        }

        q = query.SortBy?.ToLower() switch
        {
            "statusi"      => query.SortDir == "desc" ? q.OrderByDescending(t => t.Statusi)      : q.OrderBy(t => t.Statusi),
            "dateterminit" => query.SortDir == "desc" ? q.OrderByDescending(t => t.DataTerminit) : q.OrderBy(t => t.DataTerminit),
            _              => q.OrderBy(t => t.TerminId)
        };

        var total = await q.CountAsync();
        var data  = await q.Skip((query.Page - 1) * query.Limit).Take(query.Limit).ToListAsync();

        return Ok(new PagedResult<TerminResponseDto>
        {
            Data  = data.Select(ToDto).ToList(),
            Page  = query.Page,
            Limit = query.Limit,
            Total = total
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var t = await _db.Terminet
            .Include(t => t.Klienti)
            .Include(t => t.Sherbimi)
            .Include(t => t.Terapisti)
            .FirstOrDefaultAsync(t => t.TerminId == id);
        if (t == null) return NotFound();
        return Ok(ToDto(t));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TerminCreateDto dto)
    {
        var t = new Termin
        {
            KlientId = dto.KlientId,
            SherbimId = dto.SherbimId,
            TerapistId = dto.TerapistId,
            DataTerminit = dto.DataTerminit,
            OraFillimit = dto.OraFillimit,
            OraMbarimit = dto.OraMbarimit,
            Statusi = dto.Statusi,
            Shenimet = dto.Shenimet
        };
        _db.Terminet.Add(t);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Termin", "CREATE", t.TerminId.ToString(), newValues: dto);

        var created = await _db.Terminet
            .Include(x => x.Klienti)
            .Include(x => x.Sherbimi)
            .Include(x => x.Terapisti)
            .FirstAsync(x => x.TerminId == t.TerminId);
        return CreatedAtAction(nameof(GetById), new { id = t.TerminId }, ToDto(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TerminUpdateDto dto)
    {
        var t = await _db.Terminet.FindAsync(id);
        if (t == null) return NotFound();
        var old = ToDto(t);
        t.KlientId = dto.KlientId;
        t.SherbimId = dto.SherbimId;
        t.TerapistId = dto.TerapistId;
        t.DataTerminit = dto.DataTerminit;
        t.OraFillimit = dto.OraFillimit;
        t.OraMbarimit = dto.OraMbarimit;
        t.Statusi = dto.Statusi;
        t.Shenimet = dto.Shenimet;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Termin", "UPDATE", id.ToString(), oldValues: old, newValues: dto);

        var updated = await _db.Terminet
            .Include(x => x.Klienti)
            .Include(x => x.Sherbimi)
            .Include(x => x.Terapisti)
            .FirstAsync(x => x.TerminId == id);
        return Ok(ToDto(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await _db.Terminet.FindAsync(id);
        if (t == null) return NotFound();
        var old = ToDto(t);
        _db.Terminet.Remove(t);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Termin", "DELETE", id.ToString(), oldValues: old);
        return NoContent();
    }

    private static TerminResponseDto ToDto(Termin t) =>
        new(t.TerminId,
            t.KlientId, $"{t.Klienti?.Emri} {t.Klienti?.Mbiemri}",
            t.SherbimId, t.Sherbimi?.EmriSherbimit ?? "",
            t.TerapistId, $"{t.Terapisti?.Emri} {t.Terapisti?.Mbiemri}",
            t.DataTerminit, t.OraFillimit, t.OraMbarimit,
            t.Statusi, t.Shenimet);
}
