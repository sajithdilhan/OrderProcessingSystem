using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistent;

namespace OrderService.Infrastructure.Repositories;

internal class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllOrders()
    {
        return await _context.Orders.AsNoTracking().ToListAsync();
    }

    public async Task<Order> CreateOrder(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }
}