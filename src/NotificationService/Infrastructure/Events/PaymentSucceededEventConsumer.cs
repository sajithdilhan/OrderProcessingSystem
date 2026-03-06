using MassTransit;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Contracts.Enum;
using Shared.Contracts.Events;

namespace NotificationService.Infrastructure.Events;

public class PaymentSucceededEventConsumer(INotificationService notificationService, ILogger<PaymentSucceededEventConsumer> logger) : IConsumer<PaymentSucceededEvent>
{
    public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Received PaymentSucceededEvent: OrderId: {OrderId}, PaymentId: {PaymentId}, Amount: {Amount}",
            message.OrderId, message.PaymentId, message.Amount);

       // await notificationService.SendNotification(new Notification(message.Amount, message.OrderId, PaymentStatus.Pending));
    }
}
