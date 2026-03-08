using NotificationService.Application.Dto;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Contracts.Common;
using System.Net;

namespace NotificationService.Application.Services;

public class NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger) : INotificationService
{
    public async Task<Result<IEnumerable<NotificationResponse>>> GetAllNotificationsAsync()
    {
        var notifications = await notificationRepository.GetAllNotifications();
        if (!notifications.Any())
        {
            logger.LogWarning("No notifications found!");
            return Result<IEnumerable<NotificationResponse>>.Failure(new Error((int)HttpStatusCode.NotFound, "No notifications found!"));
        }

        var dtos = notifications.Select(n => NotificationResponse.ToDto(n));
        return Result<IEnumerable<NotificationResponse>>.Success(dtos);
    }

    public async Task SendNotificationAsync(Notification notification)
    {
        var existingNotification = await notificationRepository.GetNotificationById(notification.Message?.PaymentId);
        if (existingNotification != null) // Idempotency check
        {
            logger.LogWarning("Notification already exists for notification ID: {NotificationId}", notification.NotificationId);
            return;
        }

        await notificationRepository.SaveNotification(notification);
        string message = $"Payment of {notification.Message?.Amount:C} for Order {notification.Message?.OrderId} succeeded.";
        logger.LogInformation("Sending notification: {Message} to {CustomerEmail}", message, notification.Message?.CustomerEmail);
        await Task.CompletedTask;
    }
}
