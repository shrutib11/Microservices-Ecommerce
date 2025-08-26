using NotificationService.Domain.Models;

namespace NotificationService.Domain.Interfaces;

public interface IUserNotificationRepository
{
    Task<List<UserNotifications>> GetUserNotificationsAsync(int userId);

    Task<int> GetUnreadNotificationsCountAsync(int userId);

    Task<UserNotifications?> GetNotificationByUserIdAsync(int notificationId, int userId);

    Task UpdateUserNotificationAsync(UserNotifications userNotification);
}
