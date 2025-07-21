using CartService.Domain.Models;

namespace CartService.Domain.Interfaces;

public interface ICartRepository
{
    Task<Cart> Add(Cart cart);

    Task<Cart?> GetCartbyId(int cartId);

    Task<Cart?> GetUserCart(int userId);
}
