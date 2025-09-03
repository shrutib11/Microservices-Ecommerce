using Microservices.Shared.Interfaces;
using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Hubs;

namespace NotificationService.API.Services;

public class NotificationHubService : INotification
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task SendNotificationToUser(string userId, object notification)
    {
        var connectionsDict = NotificationHub.GetConnections();

        if (connectionsDict.TryGetValue(userId, out var connections))
        {
            foreach (var connId in connections)
            {
                await _hubContext.Clients.Client(connId)
                    .SendAsync("ReceiveNotification", notification);
            }
        }
    }

    public async Task BroadcastToAdmins(object notification)
    {
        await _hubContext.Clients.Group("Admins")
            .SendAsync("ReceiveNotification", notification);
    }

}
