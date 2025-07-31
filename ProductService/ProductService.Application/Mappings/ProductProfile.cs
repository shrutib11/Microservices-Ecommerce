using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Domain.Models;

namespace ProductService.Application.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<ProductDto, Product>()
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Product, ProductDto>();
    }

}


