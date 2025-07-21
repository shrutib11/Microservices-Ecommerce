using AutoMapper;
using CartService.Application.DTOs;
using CartService.Domain.Models;

namespace CartService.Application.Mappings;

public class CartProfile : Profile
{
    public CartProfile()
    {
        CreateMap<Cart, CartDto>().ReverseMap();
    }
}
