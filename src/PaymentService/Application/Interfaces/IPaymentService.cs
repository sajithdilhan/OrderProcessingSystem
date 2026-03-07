using PaymentService.Application.Dto;
using PaymentService.Domain.Entities;
using Shared.Contracts.Common;

namespace PaymentService.Application.Interfaces;

public interface IPaymentService
{
    Task<Result<IEnumerable<PaymentResponse>>> GetAllPaymentsAsync();
    Task<bool> ProcessPaymentAsync(Payment payment);
}