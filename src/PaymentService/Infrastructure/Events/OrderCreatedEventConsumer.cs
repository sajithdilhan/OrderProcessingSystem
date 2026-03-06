using MassTransit;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using Shared.Contracts.Enum;
using Shared.Contracts.Events;

namespace PaymentService.Infrastructure.Events;

public class OrderCreatedEventConsumer(IPaymentService paymentService, ILogger<OrderCreatedEventConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Received OrderCreatedEvent: OrderId: {OrderId}, CustomerEmail: {CustomerEmail}, Amount: {Amount}",
            message.OrderId, message.CustomerEmail, message.Amount);

        await paymentService.ProcessPayment(new Payment(message.Amount, message.OrderId, PaymentStatus.Pending));
    }
}
