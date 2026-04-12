using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Services;
using WellnessAPI.Models.Domain;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/programet")]
[Authorize]
public class ProgrametController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public ProgrametController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Programet.AsNoTracking().ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Programet.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(ToDto(p));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProgramCreateDto dto)
    {
        var p = new Models.Domain.Program
        {
            EmriProgramit = dto.EmriProgramit,
            Pershkrimi = dto.Pershkrimi,
            KohezgjatjaJave = dto.KohezgjatjaJave,
            Qellimi = dto.Qellimi,
            Ushtrimet = dto.Ushtrimet,
            Dieta = dto.Dieta
        };
        _db.Programet.Add(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Program", "CREATE", p.ProgramId.ToString(), newValues: dto);
        return CreatedAtAction(nameof(GetById), new { id = p.ProgramId }, ToDto(p));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProgramUpdateDto dto)
    {
        var p = await _db.Programet.FindAsync(id);
        if (p == null) return NotFound();
        var old = ToDto(p);
        p.EmriProgramit = dto.EmriProgramit;
        p.Pershkrimi = dto.Pershkrimi;
        p.KohezgjatjaJave = dto.KohezgjatjaJave;
        p.Qellimi = dto.Qellimi;
        p.Ushtrimet = dto.Ushtrimet;
        p.Dieta = dto.Dieta;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Program", "UPDATE", id.ToString(), oldValues: old, newValues: dto);
        return Ok(ToDto(p));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Programet.FindAsync(id);
        if (p == null) return NotFound();
        var old = ToDto(p);
        _db.Programet.Remove(p);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Program", "DELETE", id.ToString(), oldValues: old);
        return NoContent();
    }

    private static ProgramResponseDto ToDto(Models.Domain.Program p) =>
        new(p.ProgramId, p.EmriProgramit, p.Pershkrimi, p.KohezgjatjaJave, p.Qellimi, p.Ushtrimet, p.Dieta);
}
