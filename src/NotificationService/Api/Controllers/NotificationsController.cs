using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dto;
using NotificationService.Application.Interfaces;
using Serilog.Core;

namespace NotificationService.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(NotificationResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetNotifications() //TODO: Implement pagination
    {
        logger.LogInformation("Getting notifications.");
        var result = await notificationService.GetAllNotificationsAsync();

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.Message, statusCode: result.Error.Code);
        }

        return Ok(result.Value);
    }
}
