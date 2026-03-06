using PaymentService.Application.Dto;
using PaymentService.Domain.Entities;
using Shared.Contracts.Common;

namespace PaymentService.Application.Interfaces;

public interface IPaymentService
{
    Task<Result<IEnumerable<PaymentResponse>>> GetAllPayments();
    Task<bool> ProcessPayment(Payment payment);
}