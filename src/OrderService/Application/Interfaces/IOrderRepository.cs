using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOrderRepository
{
    public Task<IEnumerable<Order>> GetAllOrdersAsync();
    public Task<Order> CreateOrderAsync(Order order);
}
