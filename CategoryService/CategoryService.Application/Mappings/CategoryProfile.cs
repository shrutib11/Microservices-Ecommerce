using AutoMapper;
using CategoryService.Application.DTOs;
using CategoryService.Domain.Models;

namespace CategoryService.Application.Mappings;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>().ReverseMap();
    }
}
