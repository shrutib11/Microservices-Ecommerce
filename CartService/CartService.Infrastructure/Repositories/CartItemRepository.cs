using CartService.Domain.Interfaces;
using CartService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CartService.Infrastructure.Repositories;

public class CartItemRepository : ICartItemRepository
{
    private readonly CartServiceDbContext _context;

    public CartItemRepository(CartServiceDbContext context)
    {
        _context = context;
    }

    public async Task<CartItem?> GetCartItemById(int id) =>
        await _context.CartItems.FirstOrDefaultAsync(ci => ci.Id == id);

    public async Task<List<CartItem>> GetCartItemsByCartId(int cartId) =>
        await _context.CartItems.Where(ci => ci.CartId == cartId && ci.IsDeleted == false).ToListAsync();

    public async Task<CartItem> Add(CartItem cartItem)
    {
        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();
        return cartItem;
    }

    public async Task Update(CartItem cartItem)
    {
        _context.CartItems.Update(cartItem);
        await _context.SaveChangesAsync();
    }
}
