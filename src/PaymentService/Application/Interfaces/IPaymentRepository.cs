using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllPaymentsAsync();
    Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
    Task<Payment> SavePaymentAsync(Payment payment);

    Task<Payment> UpdatePaymentAsync(Payment payment);
}
