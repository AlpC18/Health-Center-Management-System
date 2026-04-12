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
[Route("api/shitjet")]
[Authorize]
public class ShitjetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public ShitjetController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10,
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var q = _db.ShitjetProduktet
            .AsNoTracking()
            .Include(s => s.Klienti)
            .Include(s => s.Produkti)
            .AsQueryable();

        if (startDate.HasValue)
            q = q.Where(s => s.DataShitjes >= startDate.Value);
        if (endDate.HasValue)
            q = q.Where(s => s.DataShitjes <= endDate.Value);

        q = q.OrderByDescending(s => s.DataShitjes);

        var total = await q.CountAsync();
        var data  = await q.Skip((page - 1) * limit).Take(limit).ToListAsync();

        return Ok(new PagedResult<ShitjeResponseDto>
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
        var s = await _db.ShitjetProduktet
            .Include(s => s.Klienti)
            .Include(s => s.Produkti)
            .FirstOrDefaultAsync(s => s.ShitjeId == id);
        if (s == null) return NotFound();
        return Ok(ToDto(s));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShitjeCreateDto dto)
    {
        var s = new ShitjeProdukteve
        {
            KlientId = dto.KlientId,
            ProduktId = dto.ProduktId,
            Sasia = dto.Sasia,
            CmimiTotal = dto.CmimiTotal,
            DataShitjes = dto.DataShitjes
        };
        _db.ShitjetProduktet.Add(s);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Shitje", "CREATE", s.ShitjeId.ToString(), newValues: dto);

        var created = await _db.ShitjetProduktet
            .Include(x => x.Klienti)
            .Include(x => x.Produkti)
            .FirstAsync(x => x.ShitjeId == s.ShitjeId);
        return CreatedAtAction(nameof(GetById), new { id = s.ShitjeId }, ToDto(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ShitjeUpdateDto dto)
    {
        var s = await _db.ShitjetProduktet.FindAsync(id);
        if (s == null) return NotFound();
        var old = new { s.KlientId, s.ProduktId, s.Sasia, s.CmimiTotal, s.DataShitjes };
        s.KlientId = dto.KlientId;
        s.ProduktId = dto.ProduktId;
        s.Sasia = dto.Sasia;
        s.CmimiTotal = dto.CmimiTotal;
        s.DataShitjes = dto.DataShitjes;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Shitje", "UPDATE", id.ToString(), oldValues: old, newValues: dto);

        var updated = await _db.ShitjetProduktet
            .Include(x => x.Klienti)
            .Include(x => x.Produkti)
            .FirstAsync(x => x.ShitjeId == id);
        return Ok(ToDto(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.ShitjetProduktet.FindAsync(id);
        if (s == null) return NotFound();
        var old = new { s.KlientId, s.ProduktId, s.Sasia, s.CmimiTotal, s.DataShitjes };
        _db.ShitjetProduktet.Remove(s);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Shitje", "DELETE", id.ToString(), oldValues: old);
        return NoContent();
    }

    private static ShitjeResponseDto ToDto(ShitjeProdukteve s) =>
        new(s.ShitjeId,
            s.KlientId, $"{s.Klienti?.Emri} {s.Klienti?.Mbiemri}",
            s.ProduktId, s.Produkti?.EmriProduktit ?? "",
            s.Sasia, s.CmimiTotal, s.DataShitjes);
}
