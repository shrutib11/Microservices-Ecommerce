using AutoMapper;
using CartService.Application.DTOs;
using CartService.Domain.Models;

namespace CartService.Application.Mappings;

public class CartItemProfile : Profile
{
    public CartItemProfile()
    {
        CreateMap<CartItem, CartItemDto>().ReverseMap();
    }
}
