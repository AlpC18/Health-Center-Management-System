using System.Security.Claims;
using System.Text.Json;
using WellnessAPI.Data;
using WellnessAPI.Models.Domain;

namespace WellnessAPI.Services;

public class AuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _http;

    public AuditService(ApplicationDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task LogAsync(string action, string entity,
        string? entityId = null, object? oldValues = null, object? newValues = null)
    {
        var user = _http.HttpContext?.User;
        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user?.FindFirst("sub")?.Value ?? "anonymous";
        var email = user?.FindFirst(ClaimTypes.Email)?.Value
            ?? user?.FindFirst("email")?.Value ?? "anonymous";

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            UserEmail = email,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = _http.HttpContext?.Connection.RemoteIpAddress?.ToString(),
        });
        await _db.SaveChangesAsync();
    }
}
