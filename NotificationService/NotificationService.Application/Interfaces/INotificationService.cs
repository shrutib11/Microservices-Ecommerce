using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationDto> AddNotificationsAsync(OrderDto orderDto);

    Task<List<NotificationDto>> GetByUserIdAsync(int userId);

    Task<int> GetUnreadCountAsync(int userId);

    Task<NotificationDto?> MarkAsReadAsync(int notificationId, int userId);
}
