using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOrderRepository
{
    public Task<IEnumerable<Order>> GetAllOrders();
    public Task<Order> CreateOrder(Order order);
}
