using OrderService.Domain.Interfaces;
using OrderService.Domain.Models;

namespace OrderService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderServiceDbContext _context;
    public OrderRepository(OrderServiceDbContext context)
    {
       _context = context;
    }

    public async Task<OrderEvent> AddOrderAsync(OrderEvent orderEvent)
    {
        await _context.OrderEvents.AddAsync(orderEvent);
        await _context.SaveChangesAsync();
        return orderEvent;
    } 
}
