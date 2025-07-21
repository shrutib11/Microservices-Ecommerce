using CartService.Domain.Interfaces;
using CartService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CartService.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CartServiceDbContext _context;

    public CartRepository(CartServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Cart> Add(Cart cart)
    {
        await _context.AddAsync(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<Cart?> GetCartbyId(int cartId) =>
        await _context.Carts.FirstOrDefaultAsync(c => c.Id == cartId && c.IsActive == true);

    public async Task<Cart?> GetUserCart(int userId) =>
        await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive == true);
}
