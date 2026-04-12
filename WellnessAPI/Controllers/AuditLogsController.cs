namespace WellnessAPI.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public AuditLogsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? entity,
        [FromQuery] string? action,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var q = _db.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(entity)) q = q.Where(a => a.Entity == entity);
        if (!string.IsNullOrEmpty(action)) q = q.Where(a => a.Action == action);
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * limit).Take(limit).ToListAsync();
        return Ok(new { data, total, page, limit });
    }
}
