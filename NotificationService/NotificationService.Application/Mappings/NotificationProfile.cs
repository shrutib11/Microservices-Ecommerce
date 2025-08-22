using AutoMapper;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Models;

namespace NotificationService.Application.Mappings;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<NotificationDto, Notifications>()
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Notifications, NotificationDto>();
    }
}
