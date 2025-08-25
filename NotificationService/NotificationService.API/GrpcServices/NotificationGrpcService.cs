using Grpc.Core;
using Microservices.Shared.Protos;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.API.GrpcServices;

public class NotificationGrpcService : Notification.NotificationBase
{
    private readonly INotificationService _notificationService;

    public NotificationGrpcService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public override async Task<OrderEventResponse> SendOrderEvent(OrderEventRequest request, ServerCallContext context)
    {
         var orderDto = new OrderDto
        {
            OrderId = request.OrderId,
            UserId = int.Parse(request.UserId), 
            Status = request.Status,
            NotificationType = (Domain.Enums.NotificationType)request.Type 
        };

        var result = await _notificationService.AddNotificationsAsync(orderDto);
        
        return new OrderEventResponse
        {
            Success = true,
            Message = $"Notification created: {result.Title}"
        };
    }
}
