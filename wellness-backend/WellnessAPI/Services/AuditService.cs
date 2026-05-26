using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using WellnessAPI.Data;
using WellnessAPI.Hubs;
using WellnessAPI.Models.Domain;

namespace WellnessAPI.Services;

public class AuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _http;
    private readonly IHubContext<NotificationHub> _hub;

    public AuditService(ApplicationDbContext db, IHttpContextAccessor http, IHubContext<NotificationHub> hub)
    {
        _db = db;
        _http = http;
        _hub = hub;
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

        // Real-time notification on every successful CRUD write. Because every
        // controller already calls LogAsync after a create/update/delete, this
        // single broadcast covers all entities without per-controller changes.
        await _hub.Clients.All.SendAsync(
            NotificationEvents.ReceiveNotification,
            BuildMessage(action, entity));
    }

    private static string BuildMessage(string action, string entity) => action switch
    {
        "CREATE" => $"{entity} u shtua",
        "UPDATE" => $"{entity} u përditësua",
        "DELETE" => $"{entity} u fshi",
        _ => $"{entity} u ndryshua",
    };
}
