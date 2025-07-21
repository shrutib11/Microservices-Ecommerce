using AutoMapper;
using CartService.Application.DTOs;
using CartService.Application.Interfaces;
using CartService.Domain.Interfaces;
using CartService.Domain.Models;

namespace CartService.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;

    private readonly ICartItemRepository _cartItemRepository;

    private readonly IMapper _mapper;

    public CartService(ICartRepository cartRepository, ICartItemRepository cartItemRepository, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _mapper = mapper;
    }

    public async Task<CartDto> Add(CartDto cartDto)
    {
        var cart = _mapper.Map<Cart>(cartDto);
        cart = await _cartRepository.Add(cart);
        return _mapper.Map<CartDto>(cart);
    }

    public async Task<CartDto?> GetCartById(int cartId)
    {
        Cart? cart = await _cartRepository.GetCartbyId(cartId);
        if (cart != null)
            return _mapper.Map<CartDto>(cart);
        return null;
    }

    public async Task<CartDto?> GetUserCart(int userId)
    {
        Cart? cart = await _cartRepository.GetUserCart(userId);
        if (cart is not null)
            return _mapper.Map<CartDto>(cart);
        return null;
    }

    public async Task<List<CartItemDto>?> GetCartItemsByCartId(int cartId)
    {
        Cart? cart = await _cartRepository.GetCartbyId(cartId);
        if (cart is null)
            return null;
        List<CartItem> cartItems = await _cartItemRepository.GetCartItemsByCartId(cartId);
        return _mapper.Map<List<CartItemDto>>(cartItems);
    }

    public async Task<bool> DeleteCartItem(int id)
    {
        CartItem? cartItem = await _cartItemRepository.GetCartItemById(id);
        if (cartItem is not null)
        {
            cartItem.IsDeleted = true;
            cartItem.UpdatedAt = DateTime.Now;
            await _cartItemRepository.Update(cartItem);
            return true;
        }
        return false;
    }

    public async Task<CartItemDto?> UpdateQuantity(CartItemDto cartItemDto)
    {
        CartItem? cartItem = await _cartItemRepository.GetCartItemById(cartItemDto.Id);
        if (cartItem is not null)
        {
            cartItem.Quantity = cartItemDto.Quantity ?? 1;
            cartItem.UpdatedAt = DateTime.Now;
            await _cartItemRepository.Update(cartItem);
            return _mapper.Map<CartItemDto>(cartItem);
        }
        return null;
    }

    public async Task<bool> ClearCart(int cartId)
    {
        Cart? cart = await _cartRepository.GetCartbyId(cartId);
        if (cart is null)
            return false;

        List<CartItem> cartItems = await _cartItemRepository.GetCartItemsByCartId(cartId);
        foreach (var ci in cartItems)
        {
            ci.IsDeleted = true;
            ci.UpdatedAt = DateTime.Now;
            await _cartItemRepository.Update(ci);
        }
        cart.IsActive = false;
        cart.UpdatedAt = DateTime.Now;
        return true;
    }

    public async Task<CartItemDto?> AddToCart(CartItemDto cartItemDto)
    {
        Cart? cart = await _cartRepository.GetCartbyId(cartItemDto.CartId);
        if (cart is not null)
        {
            var cartItem = _mapper.Map<CartItem>(cartItemDto);
            cartItem.Quantity = 1;
            cartItem = await _cartItemRepository.Add(cartItem);
            return _mapper.Map<CartItemDto>(cartItem);
        }
        return null;
    }
}
