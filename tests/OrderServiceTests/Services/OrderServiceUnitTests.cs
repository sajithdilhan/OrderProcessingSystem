using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Application.Dto;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;
using ServiceType = OrderService.Application.Services.OrderService;
using OrderService.Domain.Entities;
using Shared.Contracts.Common;
using Shared.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OrderServiceTests.Services;

public class OrderServiceUnitTests
{
    [Fact]
    public async Task GetAllOrders_ReturnsSuccess_WhenRepositoryHasOrders()
    {
        // Arrange
        var order = new Order(10, "a@b.com") { OrderId = 1 };
        var orders = new List<Order> { order };

        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.GetAllOrders()).ReturnsAsync(orders);

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.GetAllOrders();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var list = result.Value!.ToList();
        Assert.Single(list);
        Assert.Equal(1, list[0].OrderId);
        Assert.Equal(order.Amount, list[0].Amount);
        Assert.Equal(order.CustomerEmail, list[0].CustomerEmail);
    }

    [Fact]
    public async Task GetAllOrders_ReturnsFailure_WhenRepositoryReturnsNull()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.GetAllOrders()).ReturnsAsync((IEnumerable<Order>?)null);

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.GetAllOrders();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(400, result.Error!.Code);
        Assert.Equal("No orders found!", result.Error.Message);
    }

    [Fact]
    public async Task CreateOrder_ReturnsFailure_WhenRepositoryReturnsOrderWithIdZero()
    {
        // Arrange
        var request = new OrderRequest { CustomerEmail = "c@d.com", Amount = 20 };
        var returnedOrder = new Order(request.Amount, request.CustomerEmail) { OrderId = 0 };

        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.CreateOrder(It.IsAny<Order>())).ReturnsAsync(returnedOrder);

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.CreateOrder(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(500, result.Error!.Code);
        Assert.Equal("Failed to create order.", result.Error.Message);
    }

    [Fact]
    public async Task CreateOrder_PublishesEventAndReturnsSuccess_WhenRepositoryCreatesOrder()
    {
        // Arrange
        var request = new OrderRequest { CustomerEmail = "e@f.com", Amount = 50 };
        var createdOrder = new Order(request.Amount, request.CustomerEmail) { OrderId = 5, OrderDate = DateTime.UtcNow };

        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.CreateOrder(It.IsAny<Order>())).ReturnsAsync(createdOrder);

        var mockPublish = new Mock<IPublishEndpoint>();
        mockPublish.Setup(p => p.Publish(It.IsAny<OrderCreatedEvent>(), default)).Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.CreateOrder(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
        mockPublish.Verify(p => p.Publish(It.Is<OrderCreatedEvent>(e => e.OrderId == 5 && e.Amount == request.Amount && e.CustomerEmail == request.CustomerEmail), default), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_ThrowsException_WhenRepositoryThrows()
    {
        // Arrange
        var request = new OrderRequest { CustomerEmail = "x@y.com", Amount = 15 };

        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.CreateOrder(It.IsAny<Order>())).ThrowsAsync(new Exception("DB error"));

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.CreateOrder(request));
    }
}
