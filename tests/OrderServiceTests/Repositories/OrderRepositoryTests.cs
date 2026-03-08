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

    [Fact]
    public void CreatingOrder_WithNegativeAmount_ThrowsValidation()
    {
        // Arrange & Act & Assert
        Assert.Throws<OrderValidationException>(() => new Order(-5, "neg@test.com"));
    }

    [Fact]
    public async Task CreateOrder_SetsOrderDateToNow()
    {
        // Arrange
        var dbName = $"orders_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new OrderRepository(context);

        var before = DateTime.UtcNow;
        var order = new Order(15, "time@test.com");

        // Act
        var created = await repo.CreateOrderAsync(order);
        var after = DateTime.UtcNow;

        // Assert
        Assert.True(created.OrderDate >= before && created.OrderDate <= after);
    }

    [Fact]
    public async Task CreateOrder_PersistsMultipleOrders()
    {
        // Arrange
        var dbName = $"orders_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new OrderRepository(context);

        var order1 = new Order(10, "test@test.com");
        var order2 = new Order(20, "test@test.com");

        // Act
        var created1 = await repo.CreateOrderAsync(order1);
        var created2 = await repo.CreateOrderAsync(order2);

        // Assert
        var fromDb = context.Orders.ToList();
        Assert.Equal(2, fromDb.Count);
        Assert.Contains(fromDb, o => o.OrderId == created1.OrderId);
        Assert.Contains(fromDb, o => o.OrderId == created2.OrderId);
    }
}
