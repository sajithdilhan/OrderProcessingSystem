using PaymentService.Domain.Validations;
using Shared.Contracts.Enum;

namespace PaymentService.Domain.Entities;

public class Payment
{
    public int PaymentId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } 
    public decimal Amount { get; private set; }
    public int OrderId { get; private set; }

    public DateTime PaymentDate { get; private set; } = DateTime.UtcNow;

    public Guid ExternalPaymentId { get; set; }

    private Payment()
    {

    }

    public Payment(decimal amount, int orderId, PaymentStatus paymentStatus = PaymentStatus.Pending)
    {
        if (amount <= 0)
            throw new PaymentValidationException("Order amount must be greater than zero!");
        if (orderId <= 0)
            throw new PaymentValidationException("OrderId must be greater than zero!");

        Amount = amount;
        OrderId = orderId;
        PaymentStatus = paymentStatus;
        PaymentDate = DateTime.UtcNow;
    }
}
