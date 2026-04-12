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
[Route("api/sherbimet")]
[Authorize]
public class SherbiimetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public SherbiimetController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query)
    {
        var q = _db.Sherbimet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(x =>
                x.EmriSherbimit.ToLower().Contains(s) ||
                x.Kategoria.ToLower().Contains(s));
        }

        q = query.SortBy?.ToLower() switch
        {
            "emrisherbimit" => query.SortDir == "desc" ? q.OrderByDescending(x => x.EmriSherbimit) : q.OrderBy(x => x.EmriSherbimit),
            "kategoria"     => query.SortDir == "desc" ? q.OrderByDescending(x => x.Kategoria)     : q.OrderBy(x => x.Kategoria),
            "cmimi"         => query.SortDir == "desc" ? q.OrderByDescending(x => x.Cmimi)         : q.OrderBy(x => x.Cmimi),
            _               => q.OrderBy(x => x.SherbimId)
        };

        var total = await q.CountAsync();
        var data  = await q.Skip((query.Page - 1) * query.Limit).Take(query.Limit).ToListAsync();

        return Ok(new PagedResult<SherbimResponseDto>
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
        var s = await _db.Sherbimet.FindAsync(id);
        if (s == null) return NotFound();
        return Ok(ToDto(s));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SherbimCreateDto dto)
    {
        var s = new Sherbim
        {
            EmriSherbimit = dto.EmriSherbimit,
            Kategoria = dto.Kategoria,
            Pershkrimi = dto.Pershkrimi,
            KohezgjatjaMin = dto.KohezgjatjaMin,
            Cmimi = dto.Cmimi,
            Aktiv = dto.Aktiv
        };
        _db.Sherbimet.Add(s);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Sherbim", "CREATE", s.SherbimId.ToString(), newValues: dto);
        return CreatedAtAction(nameof(GetById), new { id = s.SherbimId }, ToDto(s));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SherbimUpdateDto dto)
    {
        var s = await _db.Sherbimet.FindAsync(id);
        if (s == null) return NotFound();
        var old = ToDto(s);
        s.EmriSherbimit = dto.EmriSherbimit;
        s.Kategoria = dto.Kategoria;
        s.Pershkrimi = dto.Pershkrimi;
        s.KohezgjatjaMin = dto.KohezgjatjaMin;
        s.Cmimi = dto.Cmimi;
        s.Aktiv = dto.Aktiv;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Sherbim", "UPDATE", id.ToString(), oldValues: old, newValues: dto);
        return Ok(ToDto(s));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Sherbimet.FindAsync(id);
        if (s == null) return NotFound();
        var old = ToDto(s);
        _db.Sherbimet.Remove(s);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Sherbim", "DELETE", id.ToString(), oldValues: old);
        return NoContent();
    }

    private static SherbimResponseDto ToDto(Sherbim s) =>
        new(s.SherbimId, s.EmriSherbimit, s.Kategoria, s.Pershkrimi, s.KohezgjatjaMin, s.Cmimi, s.Aktiv);
}
