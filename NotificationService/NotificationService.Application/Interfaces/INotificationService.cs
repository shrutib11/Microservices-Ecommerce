using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationDto> AddNotificationsAsync(OrderDto orderDto);
}
