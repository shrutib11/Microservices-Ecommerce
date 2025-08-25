using AutoMapper;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Interfaces;
using OrderService.Domain.Models;

namespace OrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public OrderService(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<OrderEventDto> AddOrderAsync(OrderEventDto orderEventDto)
    {
        var order = _mapper.Map<OrderEvent>(orderEventDto);
        var addedorder = await _orderRepository.AddOrderAsync(order);
        return _mapper.Map<OrderEventDto>(addedorder);
    }
}
