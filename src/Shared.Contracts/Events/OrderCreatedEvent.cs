namespace Shared.Contracts.Events;

public record OrderCreatedEvent(int OrderId, decimal Amount, string CustomerEmail, DateTime OrderDate);
