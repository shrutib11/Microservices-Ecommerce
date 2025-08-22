using AutoMapper;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Models;

namespace NotificationService.Application.Mappings;

public class UserNotificationProfile : Profile
{
    public UserNotificationProfile()
    {
        CreateMap<UserNotificationsDto, UserNotifications>()
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<UserNotifications, UserNotificationsDto>();
    }
}
