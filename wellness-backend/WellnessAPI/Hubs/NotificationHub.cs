using Microsoft.AspNetCore.SignalR;

namespace WellnessAPI.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}

public static class NotificationEvents
{
    public const string NewAppointment = "NewAppointment";
    public const string NewReview = "NewReview";
    public const string LowStock = "LowStock";
    public const string ReceiveNotification = "ReceiveNotification";
}
