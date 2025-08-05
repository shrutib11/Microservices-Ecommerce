using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Domain.Models;

namespace ProductService.Application.Mappings;

public class ProductMediaProfile : Profile
{
    public ProductMediaProfile()
    {
        CreateMap<ProductMediasDto, ProductMedia>()
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<ProductMedia, ProductMediasDto>();
    }
}
