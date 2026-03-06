namespace Shared.Contracts.Events;

public record PaymentSucceededEvent(int PaymentId, decimal Amount, int OrderId, DateTime PaymentDate);
