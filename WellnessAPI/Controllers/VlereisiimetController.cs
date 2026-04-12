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
[Route("api/vleresime")]
[Authorize]
public class VlereisiimetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public VlereisiimetController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10,
        [FromQuery] int? minNota = null, [FromQuery] int? maxNota = null)
    {
        var q = _db.Vlereisimet
            .AsNoTracking()
            .Include(v => v.Klienti)
            .Include(v => v.Sherbimi)
            .Include(v => v.Terapisti)
            .AsQueryable();

        if (minNota.HasValue)
            q = q.Where(v => v.Nota >= minNota.Value);
        if (maxNota.HasValue)
            q = q.Where(v => v.Nota <= maxNota.Value);

        q = q.OrderByDescending(v => v.DataVleresimit);

        var total = await q.CountAsync();
        var data  = await q.Skip((page - 1) * limit).Take(limit).ToListAsync();

        return Ok(new PagedResult<VleresimResponseDto>
        {
            Data  = data.Select(ToDto).ToList(),
            Page  = page,
            Limit = limit,
            Total = total
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var v = await _db.Vlereisimet
            .Include(v => v.Klienti)
            .Include(v => v.Sherbimi)
            .Include(v => v.Terapisti)
            .FirstOrDefaultAsync(v => v.VleresimId == id);
        if (v == null) return NotFound();
        return Ok(ToDto(v));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VleresimCreateDto dto)
    {
        if (dto.Nota < 1 || dto.Nota > 5)
            return BadRequest("Nota duhet te jete midis 1 dhe 5.");

        var v = new Vleresim
        {
            KlientId = dto.KlientId,
            SherbimId = dto.SherbimId,
            TerapistId = dto.TerapistId,
            Nota = dto.Nota,
            Komenti = dto.Komenti,
            DataVleresimit = dto.DataVleresimit
        };
        _db.Vlereisimet.Add(v);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Vleresim", "CREATE", v.VleresimId.ToString(), newValues: dto);

        var created = await _db.Vlereisimet
            .Include(x => x.Klienti)
            .Include(x => x.Sherbimi)
            .Include(x => x.Terapisti)
            .FirstAsync(x => x.VleresimId == v.VleresimId);
        return CreatedAtAction(nameof(GetById), new { id = v.VleresimId }, ToDto(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] VleresimUpdateDto dto)
    {
        if (dto.Nota < 1 || dto.Nota > 5)
            return BadRequest("Nota duhet te jete midis 1 dhe 5.");

        var v = await _db.Vlereisimet.FindAsync(id);
        if (v == null) return NotFound();
        var old = new { v.KlientId, v.SherbimId, v.TerapistId, v.Nota, v.Komenti, v.DataVleresimit };
        v.KlientId = dto.KlientId;
        v.SherbimId = dto.SherbimId;
        v.TerapistId = dto.TerapistId;
        v.Nota = dto.Nota;
        v.Komenti = dto.Komenti;
        v.DataVleresimit = dto.DataVleresimit;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Vleresim", "UPDATE", id.ToString(), oldValues: old, newValues: dto);

        var updated = await _db.Vlereisimet
            .Include(x => x.Klienti)
            .Include(x => x.Sherbimi)
            .Include(x => x.Terapisti)
            .FirstAsync(x => x.VleresimId == id);
        return Ok(ToDto(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var v = await _db.Vlereisimet.FindAsync(id);
        if (v == null) return NotFound();
        var old = new { v.KlientId, v.SherbimId, v.TerapistId, v.Nota, v.Komenti, v.DataVleresimit };
        _db.Vlereisimet.Remove(v);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Vleresim", "DELETE", id.ToString(), oldValues: old);
        return NoContent();
    }

    private static VleresimResponseDto ToDto(Vleresim v) =>
        new(v.VleresimId,
            v.KlientId, $"{v.Klienti?.Emri} {v.Klienti?.Mbiemri}",
            v.SherbimId, v.Sherbimi?.EmriSherbimit ?? "",
            v.TerapistId, $"{v.Terapisti?.Emri} {v.Terapisti?.Mbiemri}",
            v.Nota, v.Komenti, v.DataVleresimit);
}
