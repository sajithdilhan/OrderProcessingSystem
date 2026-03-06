using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAll();
    Task<Payment> SavePaymentAsync(Payment payment);

    Task<Payment> UpdatePaymentAsync(Payment payment);
}
