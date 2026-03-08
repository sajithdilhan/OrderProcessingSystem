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
    [Fact]
    public async Task GetAllPayments_ReturnsSuccess_WhenRepositoryHasPayments()
    {
        // Arrange
        var payment = new Payment(10m, 2, "test@test.com", paymentStatus: PaymentStatus.Pending) { PaymentId = 1 };
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
    public async Task GetAllPayments_ReturnsNotFound_WhenRepositoryReturnsEmpty()
    {
        // Arrange
        var mockRepo = new Mock<IPaymentRepository>();
        mockRepo.Setup(r => r.GetAllPaymentsAsync()).ReturnsAsync(Enumerable.Empty<Payment>());

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.GetAllPaymentsAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error!.Code);
        Assert.Equal("No payments found!", result.Error.Message);
    }

    [Fact]
    public async Task ProcessPayment_PublishesEventAndReturnsTrue_WhenPaymentSucceeds()
    {
        // Arrange
        var payment = new Payment(30m, 3, "test@test.com", paymentStatus: PaymentStatus.Pending) { PaymentId = 7 };

        var mockRepo = new Mock<IPaymentRepository>();
        mockRepo.Setup(r => r.GetPaymentByOrderIdAsync(It.IsAny<int>())).ReturnsAsync((Payment?)null);
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
        var payment = new Payment(30m, 4, "test@test.com", paymentStatus: PaymentStatus.Pending) { PaymentId = 8 };

        var mockRepo = new Mock<IPaymentRepository>();
        mockRepo.Setup(r => r.GetPaymentByOrderIdAsync(It.IsAny<int>())).ReturnsAsync((Payment?)null);
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
        var payment = new Payment(15m, 5, "test@test.com", paymentStatus: PaymentStatus.Pending) { PaymentId = 9 };

        var mockRepo = new Mock<IPaymentRepository>();
        mockRepo.Setup(r => r.GetPaymentByOrderIdAsync(It.IsAny<int>())).ReturnsAsync((Payment?)null);
        mockRepo.Setup(r => r.SavePaymentAsync(It.IsAny<Payment>())).ThrowsAsync(new Exception("DB error"));

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.ProcessPaymentAsync(payment));
    }

    [Fact]
    public async Task ProcessPayment_Throws_WhenPublishThrows()
    {
        // Arrange
        var payment = new Payment(50m, 10, "pub@throw.com", paymentStatus: Shared.Contracts.Enum.PaymentStatus.Pending) { PaymentId = 11 };

        var mockRepo = new Mock<IPaymentRepository>();
        mockRepo.Setup(r => r.GetPaymentByOrderIdAsync(It.IsAny<int>())).ReturnsAsync((Payment?)null);
        mockRepo.Setup(r => r.SavePaymentAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);
        mockRepo.Setup(r => r.UpdatePaymentAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);

        var mockPublish = new Mock<IPublishEndpoint>();
        mockPublish.Setup(p => p.Publish(It.IsAny<PaymentSucceededEvent>(), default)).ThrowsAsync(new Exception("publish failed"));

        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.ProcessPaymentAsync(payment));
    }

    [Fact]
    public async Task ProcessPayment_SetsExternalPaymentIdAndStatus_BeforeUpdate()
    {
        // Arrange
        var payment = new Payment(60m, 12, "test@test.com", paymentStatus: PaymentStatus.Pending) { PaymentId = 13 };

        var mockRepo = new Mock<IPaymentRepository>();
        mockRepo.Setup(r => r.GetPaymentByOrderIdAsync(It.IsAny<int>())).ReturnsAsync((Payment?)null);
        mockRepo.Setup(r => r.SavePaymentAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);

        Payment? updatedArg = null;
        mockRepo.Setup(r => r.UpdatePaymentAsync(It.IsAny<Payment>())).Callback<Payment>(p => updatedArg = p).ReturnsAsync((Payment p) => p);

        var mockPublish = new Mock<IPublishEndpoint>();
        mockPublish.Setup(p => p.Publish(It.IsAny<PaymentSucceededEvent>(), default)).Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.ProcessPaymentAsync(payment);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedArg);
        Assert.Equal(PaymentStatus.Completed, updatedArg!.PaymentStatus);
        Assert.NotEqual(Guid.Empty, updatedArg.ExternalPaymentId);
    }

    [Fact]
    public async Task ProcessPayment_ReturnsFalse_WhenPaymentAlreadyExists()
    {
        // Arrange
        var existingPayment = new Payment(100m, 5, "test@test.com", paymentStatus: PaymentStatus.Completed) { PaymentId = 1 };
        var newPayment = new Payment(100m, 5, "test@test.com", paymentStatus: PaymentStatus.Pending);

        var mockRepo = new Mock<IPaymentRepository>();
        mockRepo.Setup(r => r.GetPaymentByOrderIdAsync(5)).ReturnsAsync(existingPayment);

        var mockPublish = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<ServiceType>>();

        var service = new ServiceType(mockRepo.Object, mockPublish.Object, mockLogger.Object);

        // Act
        var result = await service.ProcessPaymentAsync(newPayment);

        // Assert
        Assert.False(result);
        mockRepo.Verify(r => r.SavePaymentAsync(It.IsAny<Payment>()), Times.Never);
        mockPublish.Verify(p => p.Publish(It.IsAny<PaymentSucceededEvent>(), default), Times.Never);
    }
}
