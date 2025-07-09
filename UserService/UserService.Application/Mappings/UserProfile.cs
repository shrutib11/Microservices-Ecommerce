
using AutoMapper;
namespace UserService.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<Domain.Models.User, DTOs.UserDto>()
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => new List<Domain.Models.User> { src })).ReverseMap();
        }
    }
}