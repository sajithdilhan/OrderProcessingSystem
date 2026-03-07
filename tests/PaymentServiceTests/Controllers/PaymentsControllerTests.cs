using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentService.Api.Controllers;
using PaymentService.Application.Dto;
using PaymentService.Application.Interfaces;
using Shared.Contracts.Common;

namespace PaymentServiceTests.Controllers;

public class PaymentsControllerTests
{
    [Fact]
    public async Task GetPayments_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var payments = new List<PaymentResponse>
        {
            new PaymentResponse { PaymentId = 1, Amount = 10, OrderId = 2, Status = "Completed" }
        };

        var mockService = new Mock<IPaymentService>();
        mockService.Setup(s => s.GetAllPaymentsAsync()).ReturnsAsync(Result<IEnumerable<PaymentResponse>>.Success(payments));

        var mockLogger = new Mock<ILogger<PaymentsController>>();
        var controller = new PaymentsController(mockService.Object, mockLogger.Object);

        // Act
        var result = await controller.GetPayments();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(payments, ok.Value);
    }

    [Fact]
    public async Task GetPayments_ReturnsProblem_WhenServiceFails()
    {
        // Arrange
        var error = new Error(404, "Not found");
        var mockService = new Mock<IPaymentService>();
        mockService.Setup(s => s.GetAllPaymentsAsync()).ReturnsAsync(Result<IEnumerable<PaymentResponse>>.Failure(error));

        var mockLogger = new Mock<ILogger<PaymentsController>>();
        var controller = new PaymentsController(mockService.Object, mockLogger.Object);

        // Act
        var result = await controller.GetPayments();

        // Assert
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(obj.Value);
        Assert.Equal("Not found", problem.Detail);
    }
}
