using AutoMapper;
using OrderService.Application.DTOs;
using OrderService.Domain.Models;

namespace OrderService.Application.Mappings;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<OrderEventDto, OrderEvent>()
            .ForAllMembers(opts => opts.Condition((src,dest, srcMember) => srcMember != null));

        CreateMap<OrderEvent, OrderEventDto>();
    }
}
