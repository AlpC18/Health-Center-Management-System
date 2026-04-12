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
[Route("api/produktet")]
[Authorize]
public class ProduktetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public ProduktetController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query)
    {
        var q = _db.Produktet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(p =>
                p.EmriProduktit.ToLower().Contains(s) ||
                p.Kategoria.ToLower().Contains(s));
        }

        q = query.SortBy?.ToLower() switch
        {
            "emriproduktit" => query.SortDir == "desc" ? q.OrderByDescending(p => p.EmriProduktit) : q.OrderBy(p => p.EmriProduktit),
            "kategoria"     => query.SortDir == "desc" ? q.OrderByDescending(p => p.Kategoria)     : q.OrderBy(p => p.Kategoria),
            "cmimi"         => query.SortDir == "desc" ? q.OrderByDescending(p => p.Cmimi)         : q.OrderBy(p => p.Cmimi),
            _               => q.OrderBy(p => p.ProduktId)
        };

        var total = await q.CountAsync();
        var data  = await q.Skip((query.Page - 1) * query.Limit).Take(query.Limit).ToListAsync();

        return Ok(new PagedResult<ProduktResponseDto>
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
        var p = await _db.Produktet.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(ToDto(p));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProduktCreateDto dto)
    {
        var p = new Produkt
        {
            EmriProduktit = dto.EmriProduktit,
            Kategoria = dto.Kategoria,
            Pershkrimi = dto.Pershkrimi,
            Cmimi = dto.Cmimi,
            SasiaStok = dto.SasiaStok,
            Aktiv = dto.Aktiv
        };
        _db.Produktet.Add(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Produkt", "CREATE", p.ProduktId.ToString(), newValues: dto);
        return CreatedAtAction(nameof(GetById), new { id = p.ProduktId }, ToDto(p));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProduktUpdateDto dto)
    {
        var p = await _db.Produktet.FindAsync(id);
        if (p == null) return NotFound();
        var old = ToDto(p);
        p.EmriProduktit = dto.EmriProduktit;
        p.Kategoria = dto.Kategoria;
        p.Pershkrimi = dto.Pershkrimi;
        p.Cmimi = dto.Cmimi;
        p.SasiaStok = dto.SasiaStok;
        p.Aktiv = dto.Aktiv;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Produkt", "UPDATE", id.ToString(), oldValues: old, newValues: dto);
        return Ok(ToDto(p));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Produktet.FindAsync(id);
        if (p == null) return NotFound();
        var old = ToDto(p);
        _db.Produktet.Remove(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Produkt", "DELETE", id.ToString(), oldValues: old);
        return NoContent();
    }

    private static ProduktResponseDto ToDto(Produkt p) =>
        new(p.ProduktId, p.EmriProduktit, p.Kategoria, p.Pershkrimi, p.Cmimi, p.SasiaStok, p.Aktiv);
}
