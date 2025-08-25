using System.ComponentModel.DataAnnotations;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Models;

public class OrderEvent
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string OrderId { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    [Required]
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
