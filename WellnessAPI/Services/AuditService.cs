using System.Security.Claims;
using System.Text.Json;
using WellnessAPI.Data;
using WellnessAPI.Models.Domain;

namespace WellnessAPI.Services;

public class AuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string entity, string action, string? entityId, object? oldValues = null, object? newValues = null)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user?.FindFirst("sub")?.Value;
        var userEmail = user?.FindFirst(ClaimTypes.Email)?.Value ?? user?.FindFirst("email")?.Value;

        var log = new AuditLog
        {
            Entity = entity,
            Action = action,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            UserId = userId,
            UserEmail = userEmail,
            CreatedAt = DateTime.UtcNow
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
