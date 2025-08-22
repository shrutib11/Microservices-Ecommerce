using System.ComponentModel.DataAnnotations;
using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Models;

public class Notifications
{
    [Key]
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public NotificationType Type { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
