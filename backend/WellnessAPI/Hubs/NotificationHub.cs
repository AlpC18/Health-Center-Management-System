using Microsoft.AspNetCore.SignalR;

namespace WellnessAPI.Hubs;

public class NotificationHub : Hub
{
    // Bildirimleri tüm bağlı kullanıcılara gönderir
    public async Task SendNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }
}
