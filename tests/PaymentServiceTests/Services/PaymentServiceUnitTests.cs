using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using Shared.Contracts.Enum;
using Shared.Contracts.Events;
using ServiceType = PaymentService.Application.Services.PaymentService;

namespace PaymentServiceTests.Services;

public class PaymentServiceUnitTests
{
    private const string PublishArg = "publish";

    [Fact]
    public async Task GetAllPayments_ReturnsSuccess_WhenRepositoryHasPayments()
    {
        // Arrange
        var payment = new Payment(10, 2) { PaymentId = 1 };
        var payments = new List<Payment> { payment };

        var mockRepo = new Mock<IPaymentRepository>();
        mockRepo.Setup(r => r.GetAllPaymentsAsync()).ReturnsAsync(payments);

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.GetAllPaymentsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var list = result.Value!.ToList();
        Assert.Single(list);
        Assert.Equal(1, list[0].PaymentId);
        Assert.Equal(payment.Amount, list[0].Amount);
    }

    [Fact]
    public async Task GetAllPayments_ReturnsFailure_WhenRepositoryReturnsNull()
    {
        // Arrange
        var mockRepo = new Mock<IPaymentRepository>();
        mockRepo.Setup(r => r.GetAllPaymentsAsync()).ReturnsAsync((IEnumerable<Payment>?)null);

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.GetAllPaymentsAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(400, result.Error!.Code);
        Assert.Equal("No orders found!", result.Error.Message);
    }

    [Fact]
    public async Task ProcessPayment_PublishesEventAndReturnsTrue_WhenPaymentSucceeds()
    {
        // Arrange
        var payment = new Payment(30, 3) { PaymentId = 7, PaymentStatus = PaymentStatus.Pending };

        var mockRepo = new Mock<IPaymentRepository>();
        mockRepo.Setup(r => r.SavePaymentAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);
        mockRepo.Setup(r => r.UpdatePaymentAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);

        var mockPublish = new Mock<IPublishEndpoint>();
        mockPublish.Setup(p => p.Publish(It.IsAny<PaymentSucceededEvent>(), default)).Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.ProcessPaymentAsync(payment);

        // Assert
        Assert.True(result);
        mockRepo.Verify(r => r.SavePaymentAsync(It.IsAny<Payment>()), Times.Once);
        mockRepo.Verify(r => r.UpdatePaymentAsync(It.IsAny<Payment>()), Times.Once);
        mockPublish.Verify(p => p.Publish(It.Is<PaymentSucceededEvent>(e => e.PaymentId == payment.PaymentId && e.Amount == payment.Amount && e.OrderId == payment.OrderId), default), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_ReturnsFalse_WhenUpdateMarksFailed()
    {
        // Arrange
        var payment = new Payment(30, 4) { PaymentId = 8, PaymentStatus = PaymentStatus.Pending };

        var mockRepo = new Mock<IPaymentRepository>();
        mockRepo.Setup(r => r.SavePaymentAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);
        mockRepo.Setup(r => r.UpdatePaymentAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => { p.PaymentStatus = PaymentStatus.Failed; return p; });

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.ProcessPaymentAsync(payment);

        // Assert
        Assert.False(result);
        mockPublish.Verify(p => p.Publish(It.IsAny<PaymentSucceededEvent>(), default), Times.Never);
    }

    [Fact]
    public async Task ProcessPayment_Throws_WhenSaveThrows()
    {
        // Arrange
        var payment = new Payment(15, 5) { PaymentId = 9 };

        var mockRepo = new Mock<IPaymentRepository>();
        mockRepo.Setup(r => r.SavePaymentAsync(It.IsAny<Payment>())).ThrowsAsync(new Exception("DB error"));

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.ProcessPaymentAsync(payment));
    }
}
