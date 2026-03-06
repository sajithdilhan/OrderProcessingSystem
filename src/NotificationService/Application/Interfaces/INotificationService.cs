using NotificationService.Application.Dto;
using NotificationService.Domain.Entities;
using Shared.Contracts.Common;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
   Task<Result<IEnumerable<NotificationResponse>>> GetAllNotifications();
    Task<bool> SendNotification(Notification notification);
}