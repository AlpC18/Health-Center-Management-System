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
[Route("api/terapistet")]
[Authorize]
public class TerapistetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public TerapistetController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query)
    {
        var q = _db.Terapistet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(t =>
                t.Emri.ToLower().Contains(s) ||
                t.Mbiemri.ToLower().Contains(s) ||
                t.Email.ToLower().Contains(s) ||
                t.Specializimi.ToLower().Contains(s));
        }

        q = query.SortBy?.ToLower() switch
        {
            "emri"         => query.SortDir == "desc" ? q.OrderByDescending(t => t.Emri)         : q.OrderBy(t => t.Emri),
            "email"        => query.SortDir == "desc" ? q.OrderByDescending(t => t.Email)        : q.OrderBy(t => t.Email),
            "specializimi" => query.SortDir == "desc" ? q.OrderByDescending(t => t.Specializimi) : q.OrderBy(t => t.Specializimi),
            _              => q.OrderBy(t => t.TerapistId)
        };

        var total = await q.CountAsync();
        var data  = await q.Skip((query.Page - 1) * query.Limit).Take(query.Limit).ToListAsync();

        return Ok(new PagedResult<TerapistResponseDto>
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
        var t = await _db.Terapistet.FindAsync(id);
        if (t == null) return NotFound();
        return Ok(ToDto(t));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TerapistCreateDto dto)
    {
        if (await _db.Terapistet.AnyAsync(t => t.Email == dto.Email))
            return Conflict("Ky email eshte i regjistruar.");

        var t = new Terapist
        {
            Emri = dto.Emri,
            Mbiemri = dto.Mbiemri,
            Specializimi = dto.Specializimi,
            Licenca = dto.Licenca,
            Email = dto.Email,
            Telefoni = dto.Telefoni,
            Aktiv = dto.Aktiv
        };
        _db.Terapistet.Add(t);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Terapist", "CREATE", t.TerapistId.ToString(), newValues: dto);
        return CreatedAtAction(nameof(GetById), new { id = t.TerapistId }, ToDto(t));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TerapistUpdateDto dto)
    {
        var t = await _db.Terapistet.FindAsync(id);
        if (t == null) return NotFound();

        if (await _db.Terapistet.AnyAsync(x => x.Email == dto.Email && x.TerapistId != id))
            return Conflict("Ky email eshte i regjistruar.");

        var old = ToDto(t);
        t.Emri = dto.Emri;
        t.Mbiemri = dto.Mbiemri;
        t.Specializimi = dto.Specializimi;
        t.Licenca = dto.Licenca;
        t.Email = dto.Email;
        t.Telefoni = dto.Telefoni;
        t.Aktiv = dto.Aktiv;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Terapist", "UPDATE", id.ToString(), oldValues: old, newValues: dto);
        return Ok(ToDto(t));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await _db.Terapistet.FindAsync(id);
        if (t == null) return NotFound();
        var old = ToDto(t);
        _db.Terapistet.Remove(t);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Terapist", "DELETE", id.ToString(), oldValues: old);
        return NoContent();
    }

    private static TerapistResponseDto ToDto(Terapist t) =>
        new(t.TerapistId, t.Emri, t.Mbiemri, t.Specializimi, t.Licenca, t.Email, t.Telefoni, t.Aktiv);
}
