using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs;

public class NotificationDto
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public NotificationType Type { get; set; }

    public List<UserNotificationsDto>? UserNotifications { get; set; }
}

public class UserNotificationsDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool IsRead { get; set; }

    public int NotificationId { get; set; }
}

public class OrderDto
{
    public int UserId { get; set; }

    public string OrderId { get; set; } = null!;

    public string Status { get; set; } = null!;

    public NotificationType NotificationType { get; set; }
}
