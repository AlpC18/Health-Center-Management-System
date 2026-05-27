using Microsoft.AspNetCore.SignalR;
using WellnessAPI.Data;
using WellnessAPI.Hubs;
using WellnessAPI.Models.Domain;

namespace WellnessAPI.Services;

public class NotificationService
{
    private readonly ApplicationDbContext _db;
    private readonly IHubContext<NotificationHub> _hub;

    public NotificationService(ApplicationDbContext db, IHubContext<NotificationHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task NotifyUserAsync(
        string? userId,
        string eventName,
        string title,
        string message,
        string type = "Info",
        string? link = null,
        object? payload = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;

        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Link = link,
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);

        var envelope = new
        {
            notification.NotificationId,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.Link,
            notification.IsRead,
            notification.CreatedAt,
            Payload = payload
        };

        await _hub.Clients.User(userId).SendAsync(eventName, envelope, cancellationToken);
        await _hub.Clients.User(userId).SendAsync(NotificationEvents.NotificationCreated, envelope, cancellationToken);
    }

    public async Task NotifyUsersAsync(
        IEnumerable<string?> userIds,
        string eventName,
        string title,
        string message,
        string type = "Info",
        string? link = null,
        object? payload = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
        {
            await NotifyUserAsync(userId, eventName, title, message, type, link, payload, cancellationToken);
        }
    }
}
