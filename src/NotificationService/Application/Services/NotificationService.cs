using NotificationService.Application.Dto;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Contracts.Common;
using System.Net;

namespace NotificationService.Application.Services;

public class NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger) : INotificationService
{
    public async Task<Result<IEnumerable<NotificationResponse>>> GetAllNotifications()
    {
        logger.LogInformation("Fetching all notifications");

        var notifications = await notificationRepository.GetAllNotifications();
        if (notifications is null)
        {
            logger.LogWarning("No notifications found!");
            return Result<IEnumerable<NotificationResponse>>.Failure(new Error((int)HttpStatusCode.BadRequest, "No notifications found!"));
        }

        logger.LogInformation("Returning notifications.");
        var dtos = notifications.Select(n => NotificationResponse.ToDto(n));
        return Result<IEnumerable<NotificationResponse>>.Success(dtos);
    }

    public Task<bool> SendNotification(Notification notification)
    {
        throw new NotImplementedException();
    }
}
