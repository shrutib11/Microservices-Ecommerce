using CartService.Application.DTOs;

namespace CartService.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> Add(CartDto cartDto);

    Task<CartDto?> GetCartById(int cartId);

    Task<CartDto?> GetUserCart(int userId);

    Task<bool> DeleteCartItem(int id);

    Task<bool> ClearCart(int cartId);

    Task<CartItemDto?> UpdateQuantity(UpdateCartItemQuantityDto cartItemDto);

    Task<List<CartItemDto>?> GetCartItemsByCartId(int cartId);

    Task<CartItemDto?> AddToCart(CartItemDto cartItemDto);
}
