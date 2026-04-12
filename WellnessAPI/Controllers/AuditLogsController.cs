using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.Helpers;
using WellnessAPI.Models.Domain;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/auditlogs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AuditLogsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? entity = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var q = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entity))
            q = q.Where(a => a.Entity == entity);

        if (!string.IsNullOrWhiteSpace(action))
            q = q.Where(a => a.Action == action);

        if (from.HasValue)
            q = q.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            q = q.Where(a => a.CreatedAt <= to.Value);

        q = q.OrderByDescending(a => a.CreatedAt);

        var total = await q.CountAsync();
        var data  = await q.Skip((page - 1) * limit).Take(limit).ToListAsync();

        return Ok(new PagedResult<AuditLog>
        {
            Data  = data,
            Page  = page,
            Limit = limit,
            Total = total
        });
    }
}
