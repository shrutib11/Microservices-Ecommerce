using OrderService.Domain.Models;

namespace OrderService.Domain.Interfaces;

public interface IOrderRepository
{
    Task<OrderEvent> AddOrderAsync(OrderEvent orderEvent);
}
