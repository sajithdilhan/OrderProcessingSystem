using MassTransit;
using PaymentService.Application.Dto;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using Shared.Contracts.Common;
using Shared.Contracts.Enum;
using Shared.Contracts.Events;
using System.Net;

namespace PaymentService.Application.Services;

public class PaymentService(IPaymentRepository repository, IPublishEndpoint publishEndpoint, ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<Result<IEnumerable<PaymentResponse>>> GetAllPaymentsAsync()
    {
        var payments = await repository.GetAllPaymentsAsync();
        if (!payments.Any())
        {
            logger.LogWarning("No payments found!");
            return Result<IEnumerable<PaymentResponse>>.Failure(new Error((int)HttpStatusCode.NotFound, "No payments found!"));
        }

        logger.LogInformation("Returning payments.");
        var dtos = payments.Select(p => PaymentResponse.ToDto(p));
        return Result<IEnumerable<PaymentResponse>>.Success(dtos);
    }

    public async Task<bool> ProcessPaymentAsync(Payment payment)
    {
        var existingPayment = await repository.GetPaymentByOrderIdAsync(payment.OrderId);
        if (existingPayment != null) // Idempotency check
        {
            logger.LogWarning("Payment already exists for order ID: {OrderId}", payment.OrderId);
            return false;
        }

        await repository.SavePaymentAsync(payment);
        await CallExternalPaymentService(payment);
        await repository.UpdatePaymentAsync(payment);
        if (payment.PaymentStatus == PaymentStatus.Failed)
        {
            logger.LogWarning("Payment processing failed for payment ID: {PaymentId}", payment.PaymentId);
            return false;
        }

        await PublishPaymentSuccessEvent(payment);
        return true;
    }

    private async Task PublishPaymentSuccessEvent(Payment payment)
    {
        await publishEndpoint.Publish(new PaymentSucceededEvent(payment.PaymentId, payment.Amount, payment.OrderId, payment.PaymentDate, payment.CustomerEmail));
        logger.LogInformation("PaymentSucceededEvent published for payment ID: {PaymentId}", payment.PaymentId);
    }

    private async Task<Payment> CallExternalPaymentService(Payment payment)
    {
        logger.LogInformation("External payment service call completed for payment ID: {PaymentId}", payment.PaymentId);
        payment.PaymentStatus = PaymentStatus.Completed;
        payment.ExternalPaymentId = Guid.NewGuid();
        return payment;
    }
}
