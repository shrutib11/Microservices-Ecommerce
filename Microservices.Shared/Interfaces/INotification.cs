namespace Microservices.Shared.Interfaces;

public interface INotification
{
    Task SendNotificationToUser(string userId, object message);

    Task BroadcastToAdmins(object message);

}
