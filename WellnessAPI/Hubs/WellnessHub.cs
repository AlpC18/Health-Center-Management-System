using Microsoft.AspNetCore.SignalR;

namespace WellnessAPI.Hubs;

public class WellnessHub : Hub
{
    public async Task SendNotification(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", user, message);
    }
}
