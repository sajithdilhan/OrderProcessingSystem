using OrderService.Domain.Validations;

namespace OrderService.Domain.Entities;

public class Order
{
    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    private Order()
    {

    }

    public Order(decimal amount, string email)
    {
        if (amount <= 0)
            throw new OrderValidationException("Order amount must be greater than zero!");

        Amount = amount;
        CustomerEmail = email;
        OrderDate = DateTime.UtcNow;
    }
}
