using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Helpers;
using WellnessAPI.Models.Domain;
using WellnessAPI.Services;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KlientetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;

    public KlientetController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query)
    {
        var q = _db.Klientet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(k =>
                k.Emri.ToLower().Contains(s) ||
                k.Mbiemri.ToLower().Contains(s) ||
                k.Email.ToLower().Contains(s) ||
                k.Telefoni.ToLower().Contains(s));
        }

        q = query.SortBy?.ToLower() switch
        {
            "emri"             => query.SortDir == "desc" ? q.OrderByDescending(k => k.Emri)             : q.OrderBy(k => k.Emri),
            "email"            => query.SortDir == "desc" ? q.OrderByDescending(k => k.Email)            : q.OrderBy(k => k.Email),
            "dataregjistrimit" => query.SortDir == "desc" ? q.OrderByDescending(k => k.DataRegjistrimit) : q.OrderBy(k => k.DataRegjistrimit),
            _                  => q.OrderBy(k => k.KlientId)
        };

        var total = await q.CountAsync();
        var data  = await q.Skip((query.Page - 1) * query.Limit).Take(query.Limit).ToListAsync();

        return Ok(new PagedResult<KlientResponseDto>
        {
            Data  = data.Select(k => ToDto(k)).ToList(),
            Page  = query.Page,
            Limit = query.Limit,
            Total = total
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var k = await _db.Klientet.FindAsync(id);
        if (k == null) return NotFound();
        return Ok(ToDto(k));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] KlientCreateDto dto)
    {
        if (await _db.Klientet.AnyAsync(k => k.Email == dto.Email))
            return Conflict("Ky email eshte i regjistruar.");

        var klient = new Klient
        {
            Emri = dto.Emri,
            Mbiemri = dto.Mbiemri,
            Email = dto.Email,
            Telefoni = dto.Telefoni,
            DataLindjes = dto.DataLindjes,
            Gjinia = dto.Gjinia,
            KushtetShendetesore = dto.KushtetShendetesore
        };

        _db.Klientet.Add(klient);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Klient", "CREATE", klient.KlientId.ToString(), newValues: dto);
        return CreatedAtAction(nameof(GetById), new { id = klient.KlientId }, ToDto(klient));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] KlientUpdateDto dto)
    {
        var klient = await _db.Klientet.FindAsync(id);
        if (klient == null) return NotFound();

        if (await _db.Klientet.AnyAsync(k => k.Email == dto.Email && k.KlientId != id))
            return Conflict("Ky email eshte i regjistruar.");

        var old = ToDto(klient);

        klient.Emri = dto.Emri;
        klient.Mbiemri = dto.Mbiemri;
        klient.Email = dto.Email;
        klient.Telefoni = dto.Telefoni;
        klient.DataLindjes = dto.DataLindjes;
        klient.Gjinia = dto.Gjinia;
        klient.KushtetShendetesore = dto.KushtetShendetesore;

        await _db.SaveChangesAsync();
        await _audit.LogAsync("Klient", "UPDATE", id.ToString(), oldValues: old, newValues: dto);
        return Ok(ToDto(klient));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var klient = await _db.Klientet.FindAsync(id);
        if (klient == null) return NotFound();
        var old = ToDto(klient);
        _db.Klientet.Remove(klient);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Klient", "DELETE", id.ToString(), oldValues: old);
        return NoContent();
    }

    private static KlientResponseDto ToDto(Klient k) =>
        new(k.KlientId, k.Emri, k.Mbiemri, k.Email,
            k.Telefoni, k.DataLindjes, k.Gjinia,
            k.KushtetShendetesore, k.DataRegjistrimit);
}
