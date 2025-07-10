
using AutoMapper;
namespace UserService.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<Domain.Models.User, DTOs.UserDto>()
                .ReverseMap();
        }
    }
}