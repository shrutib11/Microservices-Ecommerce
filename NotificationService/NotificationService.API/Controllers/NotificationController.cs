using System.Net;
using Microservices.Shared.Helpers;
using Microservices.Shared.Protos;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.API.Controllers;

[Route("api/notification")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    private readonly User.UserClient _userClient;

    public NotificationController(INotificationService notificationService, User.UserClient userClient)
    {
        _notificationService = notificationService;
        _userClient = userClient;
    }

    // [HttpPost("Add")]
    // [ProducesResponseType(StatusCodes.Status201Created)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    // public async Task<IActionResult> AddNotifications([FromForm] OrderDto orderDto)
    // {
    //     if (!ModelState.IsValid)
    //     {
    //         var errors = ModelState
    //             .Where(ms => ms.Value!.Errors.Count > 0)
    //             .ToDictionary(
    //                 kvp => kvp.Key,
    //                 kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList()
    //             );
    //         return BadRequest(ApiResponseHelper.Error("Validation Failed", HttpStatusCode.BadRequest, errors));
    //     }

    //     var response = await _userClient.GetUserByIdAsync(new GetUserByIdRequest
    //     {
    //         UserId = orderDto.UserId
    //     });

    //     if (!response.IsFound)
    //     {
    //         return BadRequest(ApiResponseHelper.Error("User does not exist.", HttpStatusCode.BadRequest));
    //     }

    //     var addedNotification = await _notificationService.AddNotificationsAsync(orderDto);
    //     return Ok(ApiResponseHelper.Success(addedNotification, HttpStatusCode.Created));
    // }
}
