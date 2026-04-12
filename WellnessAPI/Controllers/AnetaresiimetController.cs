using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Services;
using WellnessAPI.Models.Domain;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/anetaresimet")]
[Authorize]
public class AnetaresiimetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public AnetaresiimetController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Anetaresimet
            .AsNoTracking()
            .Include(a => a.Klienti)
            .Include(a => a.Paketa)
            .ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var a = await _db.Anetaresimet
            .Include(a => a.Klienti)
            .Include(a => a.Paketa)
            .FirstOrDefaultAsync(a => a.AnetaresimId == id);
        if (a == null) return NotFound();
        return Ok(ToDto(a));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AnetaresimCreateDto dto)
    {
        var a = new Anetaresim
        {
            KlientId = dto.KlientId,
            PaketId = dto.PaketId,
            DataFillimit = dto.DataFillimit,
            DataMbarimit = dto.DataMbarimit,
            Statusi = dto.Statusi,
            CmimiPaguar = dto.CmimiPaguar
        };
        _db.Anetaresimet.Add(a);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Anetaresim", "CREATE", a.AnetaresimId.ToString(), newValues: dto);

        var created = await _db.Anetaresimet
            .Include(x => x.Klienti)
            .Include(x => x.Paketa)
            .FirstAsync(x => x.AnetaresimId == a.AnetaresimId);
        return CreatedAtAction(nameof(GetById), new { id = a.AnetaresimId }, ToDto(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AnetaresimUpdateDto dto)
    {
        var a = await _db.Anetaresimet.FindAsync(id);
        if (a == null) return NotFound();
        var old = ToDto(a);
        a.KlientId = dto.KlientId;
        a.PaketId = dto.PaketId;
        a.DataFillimit = dto.DataFillimit;
        a.DataMbarimit = dto.DataMbarimit;
        a.Statusi = dto.Statusi;
        a.CmimiPaguar = dto.CmimiPaguar;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Anetaresim", "UPDATE", id.ToString(), oldValues: old, newValues: dto);

        var updated = await _db.Anetaresimet
            .Include(x => x.Klienti)
            .Include(x => x.Paketa)
            .FirstAsync(x => x.AnetaresimId == id);
        return Ok(ToDto(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.Anetaresimet.FindAsync(id);
        if (a == null) return NotFound();
        var old = ToDto(a);
        _db.Anetaresimet.Remove(a);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Anetaresim", "DELETE", id.ToString(), oldValues: old);
        return NoContent();
    }

    private static AnetaresimResponseDto ToDto(Anetaresim a) =>
        new(a.AnetaresimId,
            a.KlientId, $"{a.Klienti?.Emri} {a.Klienti?.Mbiemri}",
            a.PaketId, a.Paketa?.EmriPaketes ?? "",
            a.DataFillimit, a.DataMbarimit, a.Statusi, a.CmimiPaguar);
}
