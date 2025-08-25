using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface IOrderService
{
    Task<OrderEventDto> AddOrderAsync(OrderEventDto orderEventdto);
}
