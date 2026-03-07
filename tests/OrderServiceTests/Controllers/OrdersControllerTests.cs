using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Api.Controllers;
using OrderService.Application.Dto;
using OrderService.Application.Interfaces;
using Shared.Contracts.Common;

namespace OrderServiceTests.Controllers;

public class OrdersControllerTests
{
    [Fact]
    public async Task GetOrders_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var orders = new List<OrderResponse>
        {
            new OrderResponse { OrderId = 1, Amount = 10, CustomerEmail = "a@b.com" }
        };

        var mockService = new Mock<IOrderService>();
        mockService.Setup(s => s.GetAllOrders()).ReturnsAsync(Result<IEnumerable<OrderResponse>>.Success(orders));

        var mockLogger = new Mock<ILogger<OrdersController>>();
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        // Act
        var result = await controller.GetOrders();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(orders, ok.Value);
    }

    [Fact]
    public async Task GetOrders_ReturnsProblem_WhenServiceFails()
    {
        // Arrange
        var error = new Error(404, "Not found");
        var mockService = new Mock<IOrderService>();
        mockService.Setup(s => s.GetAllOrders()).ReturnsAsync(Result<IEnumerable<OrderResponse>>.Failure(error));

        var mockLogger = new Mock<ILogger<OrdersController>>();
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        // Act
        var result = await controller.GetOrders();

        // Assert
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(obj.Value);
        Assert.Equal("Not found", problem.Detail);
    }

    [Fact]
    public async Task CreateOrders_ReturnsCreated_WhenServiceSucceeds()
    {
        // Arrange
        var request = new OrderRequest { CustomerEmail = "c@d.com", Amount = 20 };
        var mockService = new Mock<IOrderService>();
        mockService.Setup(s => s.CreateOrder(request)).ReturnsAsync(Result<int>.Success(123));

        var mockLogger = new Mock<ILogger<OrdersController>>();
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        // Act
        var result = await controller.CreateOrders(request);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(123, created.Value);
    }

    [Fact]
    public async Task CreateOrders_ReturnsProblem_WhenServiceFails()
    {
        // Arrange
        var request = new OrderRequest { CustomerEmail = "c@d.com", Amount = 20 };
        var error = new Error(400, "Bad request");
        var mockService = new Mock<IOrderService>();
        mockService.Setup(s => s.CreateOrder(request)).ReturnsAsync(Result<int>.Failure(error));

        var mockLogger = new Mock<ILogger<OrdersController>>();
        var controller = new OrdersController(mockService.Object, mockLogger.Object);

        // Act
        var result = await controller.CreateOrders(request);

        // Assert
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, obj.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(obj.Value);
        Assert.Equal("Bad request", problem.Detail);
    }
}
