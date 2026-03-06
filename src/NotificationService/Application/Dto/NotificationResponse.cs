namespace NotificationService.Application.Dto
{
    public class NotificationResponse
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public static NotificationResponse ToDto(Domain.Entities.Notification notification)
        {
            return new NotificationResponse
            {
                NotificationId = notification.NotificationId,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}