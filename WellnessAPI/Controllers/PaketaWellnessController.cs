using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Services;
using WellnessAPI.Models.Domain;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/paketawellness")]
[Authorize]
public class PaketaWellnessController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public PaketaWellnessController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.PaketaWellness.AsNoTracking().ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.PaketaWellness.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(ToDto(p));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PaketaCreateDto dto)
    {
        var p = new PaketaWellness
        {
            EmriPaketes = dto.EmriPaketes,
            Pershkrimi = dto.Pershkrimi,
            SherbimiPerfshire = dto.SherbimiPerfshire,
            Cmimi = dto.Cmimi,
            KohezgjatjaMuaj = dto.KohezgjatjaMuaj,
            Aktive = dto.Aktive
        };
        _db.PaketaWellness.Add(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Paketa", "CREATE", p.PaketId.ToString(), newValues: dto);
        return CreatedAtAction(nameof(GetById), new { id = p.PaketId }, ToDto(p));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PaketaUpdateDto dto)
    {
        var p = await _db.PaketaWellness.FindAsync(id);
        if (p == null) return NotFound();
        var old = ToDto(p);
        p.EmriPaketes = dto.EmriPaketes;
        p.Pershkrimi = dto.Pershkrimi;
        p.SherbimiPerfshire = dto.SherbimiPerfshire;
        p.Cmimi = dto.Cmimi;
        p.KohezgjatjaMuaj = dto.KohezgjatjaMuaj;
        p.Aktive = dto.Aktive;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Paketa", "UPDATE", id.ToString(), oldValues: old, newValues: dto);
        return Ok(ToDto(p));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.PaketaWellness.FindAsync(id);
        if (p == null) return NotFound();
        var old = ToDto(p);
        _db.PaketaWellness.Remove(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Paketa", "DELETE", id.ToString(), oldValues: old);
        return NoContent();
    }

    private static PaketaResponseDto ToDto(PaketaWellness p) =>
        new(p.PaketId, p.EmriPaketes, p.Pershkrimi, p.SherbimiPerfshire, p.Cmimi, p.KohezgjatjaMuaj, p.Aktive);
}
