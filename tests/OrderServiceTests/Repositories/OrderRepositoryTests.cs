using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Validations;
using OrderService.Infrastructure.Persistent;
using OrderService.Infrastructure.Repositories;

namespace OrderServiceTests.Repositories;

public class OrderRepositoryTests
{
    private OrdersDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new OrdersDbContext(options);
    }

    [Fact]
    public async Task GetAllOrders_ReturnsEmpty_WhenNoOrders()
    {
        // Arrange
        var dbName = $"orders_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new OrderRepository(context);

        // Act
        var list = await repo.GetAllOrdersAsync();

        // Assert
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public async Task CreateOrder_PersistsOrderAndAssignsId()
    {
        // Arrange
        var dbName = $"orders_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new OrderRepository(context);

        var order = new Order(25, "test@test.com");

        // Act
        var created = await repo.CreateOrderAsync(order);

        // Assert
        Assert.NotNull(created);
        Assert.True(created.OrderId > 0);

        var fromDb = context.Orders.ToList();
        Assert.Single(fromDb);
        Assert.Equal(created.OrderId, fromDb[0].OrderId);
    }

    [Fact]
    public void CreatingOrder_WithInvalidAmount_ThrowsValidation()
    {
        // Arrange & Act & Assert
        Assert.Throws<OrderValidationException>(() => new Order(0, "test@test.com"));
    }
}
