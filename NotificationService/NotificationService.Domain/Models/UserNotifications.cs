using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotificationService.Domain.Models;

public class UserNotifications
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey("Notifications")]
    public int NotificationId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Notifications Notifications { get; set; } = null!;
}
