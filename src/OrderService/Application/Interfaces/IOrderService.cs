using OrderService.Application.Dto;
using Shared.Contracts.Common;

namespace OrderService.Application.Interfaces;

public interface IOrderService
{
    Task<Result<IEnumerable<OrderResponse>>> GetAllOrdersAsync();
    Task<Result<int>> CreateOrderAsync(OrderRequest request);
}
