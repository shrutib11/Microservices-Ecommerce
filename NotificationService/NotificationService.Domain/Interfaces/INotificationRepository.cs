using NotificationService.Domain.Models;

namespace NotificationService.Domain.Interfaces;

public interface INotificationRepository
{
    Task<(Notifications notifications, List<UserNotifications> userNotifications)>
        AddNotificationsAsync(Notifications notifications, List<UserNotifications> userNotifications);
}
