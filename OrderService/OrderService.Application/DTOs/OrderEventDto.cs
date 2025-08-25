using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs;

public class OrderEventDto
{
    public int? Id { get; set; }
    public int UserId { get; set; }
    public OrderStatus Status { get; set; }
    public string? OrderId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
