using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetAllNotifications();
    Task<Notification> SaveNotification(Notification notification);
}
