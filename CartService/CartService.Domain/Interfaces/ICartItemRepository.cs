using CartService.Domain.Models;

namespace CartService.Domain.Interfaces;

public interface ICartItemRepository
{
    Task<CartItem?> GetCartItemById(int id);

    Task Update(CartItem cartItem);

    Task<CartItem> Add(CartItem cartItem);

    Task<List<CartItem>> GetCartItemsByCartId(int cartId);
}
