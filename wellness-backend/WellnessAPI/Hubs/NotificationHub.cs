using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WellnessAPI.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public async Task SendNotification(string message)
    {
        await Clients.Caller.SendAsync("ReceiveNotification", message);
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.User(user).SendAsync("ReceiveMessage", Context.UserIdentifier ?? "system", message);
    }
}

public static class NotificationEvents
{
    public const string NotificationCreated = "NotificationCreated";
    public const string NewAppointment = "NewAppointment";
    public const string RescheduleProposed = "RescheduleProposed";
    public const string RescheduleApproved = "RescheduleApproved";
    public const string RescheduleDeclined = "RescheduleDeclined";
    public const string NewReview = "NewReview";
    public const string LowStock = "LowStock";
    public const string ReceiveNotification = "ReceiveNotification";
}
