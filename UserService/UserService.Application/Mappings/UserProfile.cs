
using AutoMapper;
namespace UserService.Application.Mappings
{
    public class UserProfile : Profile
    {
       public UserProfile()
        {
             CreateMap<Domain.Models.User, DTOs.UserDto>()
                .ReverseMap()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember, destMember, context) =>
                        srcMember != null &&
                        (srcMember is not string str || !string.IsNullOrWhiteSpace(str))));
        }
    }
}