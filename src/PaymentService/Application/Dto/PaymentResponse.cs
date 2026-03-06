using PaymentService.Domain.Entities;

namespace PaymentService.Application.Dto;

public class PaymentResponse
{
    public int PaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int OrderId { get; set; }
    public string PaymentDate { get; set; } = string.Empty;    
    public static PaymentResponse ToDto(Payment payment)
    {
        return new PaymentResponse
        {
            PaymentId = payment.PaymentId,
            Status = payment.PaymentStatus.ToString(),
            Amount = payment.Amount,
            OrderId = payment.OrderId,
            PaymentDate = payment.PaymentDate.ToShortDateString(),
        };
    }

}
