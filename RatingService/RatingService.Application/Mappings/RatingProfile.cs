using AutoMapper;
using RatingService.Application.DTOs;
using RatingService.Domain.Models;

namespace RatingService.Application.Mappings;

public class RatingProfile : Profile
{
    public RatingProfile()
    {
        CreateMap<RatingDto, Rating>().ReverseMap();
    }
}
