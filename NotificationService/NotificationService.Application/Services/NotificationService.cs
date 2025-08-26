using AutoMapper;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Models;

namespace NotificationService.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    private readonly IUserNotificationRepository _userNotificationRepository;

    private readonly IMapper _mapper;

    public NotificationService(INotificationRepository notificationRepository, IUserNotificationRepository userNotificationRepository, IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _userNotificationRepository = userNotificationRepository;
        _mapper = mapper;
    }

    public async Task<NotificationDto> AddNotificationsAsync(OrderDto orderDto)
    {
        string title = string.Empty;
        string message = string.Empty;

        if (orderDto.NotificationType == Domain.Enums.NotificationType.Order)
        {
            switch (orderDto.Status)
            {
                case "Shipped":
                    title = "Order Shipped Successfully";
                    message = $"Your order #{orderDto.OrderId} has been shipped.";
                    break;

                case "Delivered":
                    title = "Order Delivered";
                    message = $"Your order #{orderDto.OrderId} has been delivered.";
                    break;

                default:
                    title = "Order Update";
                    message = $"Your order #{orderDto.OrderId} status is updated to {orderDto.Status}.";
                    break;
            }
        }
        else
        {
            title = "Notification";
            message = $"You have a new notification regarding order #{orderDto.OrderId}.";
        }
        var notification = new Notifications
        {
            Title = title,
            Message = message,
            Type = orderDto.NotificationType
        };

        var userNotificationsList = new List<UserNotifications>();
        var userNotification = new UserNotifications
        {
            UserId = orderDto.UserId,
        };
        userNotificationsList.Add(userNotification);

        var savedNotification = await _notificationRepository.AddNotificationsAsync(notification, userNotificationsList);

        var resultDto = _mapper.Map<NotificationDto>(savedNotification.notifications);
        resultDto.UserNotifications = _mapper.Map<List<UserNotificationsDto>>(savedNotification.userNotifications);

        return resultDto;
    }

    public async Task<List<NotificationDto>> GetByUserIdAsync(int userId)
    {
        var userNotifications = await _userNotificationRepository.GetUserNotificationsAsync(userId);

        var notificationDtos = userNotifications.Select(un =>
        {
            var notificationDto = _mapper.Map<NotificationDto>(un.Notifications);

            notificationDto.UserNotifications = new List<UserNotificationsDto>
            {
                _mapper.Map<UserNotificationsDto>(un)
            };

            return notificationDto;
        }).ToList();

        return notificationDtos;
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _userNotificationRepository.GetUnreadNotificationsCountAsync(userId);
    }

    public async Task<NotificationDto?> MarkAsReadAsync(int notificationId, int userId)
    {
        var userNotification = await _userNotificationRepository.GetNotificationByUserIdAsync(notificationId, userId);

        if (userNotification == null)
            return null;

        userNotification.IsRead = true;
        userNotification.UpdatedAt = DateTime.UtcNow;

        await _userNotificationRepository.UpdateUserNotificationAsync(userNotification);

        var notification = await _notificationRepository.GetByIdAsync(userNotification.NotificationId);
        if (notification == null) return null;

        var notificationDto = _mapper.Map<NotificationDto>(notification);

        var userNotificationsDto = _mapper.Map<UserNotificationsDto>(userNotification);

        notificationDto.UserNotifications = new List<UserNotificationsDto>
        {
            userNotificationsDto
        };

        return notificationDto;
    }
}
