using System.Net;
using Microservices.Shared.Helpers;
using Microservices.Shared.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Enums;

namespace OrderService.API.Controllers;

// [ApiController]
// [Authorize]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly Notification.NotificationClient _notificationClient;
    public OrderController(IOrderService orderService, Notification.NotificationClient notificationClient)
    {
        _notificationClient = notificationClient;
        _orderService = orderService;
    }

    [HttpPost("Create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromForm] OrderEventDto orderEventDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(ms => ms.Value!.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                );
            return BadRequest(ApiResponseHelper.Error("Validation Failed", HttpStatusCode.BadRequest, errors));
        }

        var createdOrder = await _orderService.AddOrderAsync(orderEventDto);

        if (createdOrder == null)
        {
            return BadRequest(ApiResponseHelper.Error("Failed to create order", HttpStatusCode.BadRequest));
        }

        var response = _notificationClient.SendOrderEvent(new OrderEventRequest
        {
            OrderId = createdOrder.OrderId,
            UserId = createdOrder.UserId.ToString(),
            Status = createdOrder.Status.ToString(),
            Type = NotificationType.Order
        });

        if (response == null || !response.Success)
        {
            return BadRequest(ApiResponseHelper.Error("Failed to send notification", HttpStatusCode.BadRequest));
        }

        return Ok(ApiResponseHelper.Success(createdOrder, HttpStatusCode.Created));
    }
}
