using NotificationService.Application.Dto;

namespace NotificationService.Domain.Entities;

public class Notification
{
    public int NotificationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    private Notification()
    {
        
    }

    public Notification(string message)
    {
        Message = message;
        CreatedAt = DateTime.UtcNow;
    }
}
