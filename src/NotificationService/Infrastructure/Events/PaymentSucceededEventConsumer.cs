using MassTransit;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Contracts.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Events;

public class PaymentSucceededEventConsumer(INotificationService notificationService, ILogger<PaymentSucceededEventConsumer> logger) : IConsumer<PaymentSucceededEvent>
{
    public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Received PaymentSucceededEvent: {Payment}", JsonSerializer.Serialize(message));

        string notificationMessage = $"Payment of {message.Amount:C} for Order {message.OrderId} succeeded on {message.PaymentDate}.";

        await notificationService.SendNotification(new Notification(notificationMessage));
    }
}
