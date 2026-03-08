using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Contracts.Events;
using ServiceType = NotificationService.Application.Services.NotificationService;

namespace NotificationServiceTests.Services;

public class NotificationServiceUnitTests
{
    [Fact]
    public async Task GetAllNotifications_ReturnsSuccess_WhenRepositoryHasNotifications()
    {
        // Arrange
        var message = new PaymentSucceededEvent(1, 100m, 1, DateTime.UtcNow, "test@example.com");
        var notification = new Notification(message);
        notification.NotificationId = 1;
        var notifications = new List<Notification> { notification };

        var mockRepo = new Mock<INotificationRepository>();
        mockRepo.Setup(r => r.GetAllNotifications()).ReturnsAsync(notifications);

        var mockLogger = new Mock<ILogger<ServiceType>>();
        var service = new ServiceType(mockRepo.Object, mockLogger.Object);

        // Act
        var result = await service.GetAllNotificationsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var list = result.Value!.ToList();
        Assert.Single(list);
        Assert.Equal(1, list[0].NotificationId);
    }

    [Fact]
    public async Task GetAllNotifications_ReturnsFailure_WhenRepositoryReturnsEmpty()
    {
        // Arrange
        var mockRepo = new Mock<INotificationRepository>();
        mockRepo.Setup(r => r.GetAllNotifications())
        .ReturnsAsync(Enumerable.Empty<Notification>());

        var mockLogger = new Mock<ILogger<ServiceType>>();
        var service = new ServiceType(mockRepo.Object, mockLogger.Object);

        // Act
        var result = await service.GetAllNotificationsAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error!.Code);
        Assert.Equal("No notifications found!", result.Error.Message);
    }

    [Fact]
    public async Task SendNotification_CallsRepository_Save_WhenCalled()
    {
        // Arrange
        var notification = new Notification(new PaymentSucceededEvent(1, 100m, 1, DateTime.UtcNow, "test@example.com")) { NotificationId = 0 };

        var mockRepo = new Mock<INotificationRepository>();
        mockRepo.Setup(r => r.SaveNotification(It.IsAny<Notification>())).ReturnsAsync((Notification n) => { n.NotificationId = 2; return n; });

        var mockLogger = new Mock<ILogger<ServiceType>>();
        var service = new ServiceType(mockRepo.Object, mockLogger.Object);

        // Act
        await service.SendNotificationAsync(notification);

        // Assert
        mockRepo.Verify(r => r.SaveNotification(It.IsAny<Notification>()), Times.Once);
    }

    [Fact]
    public async Task SendNotification_Throws_WhenRepositoryThrows()
    {
        // Arrange
        var notification = new Notification(new PaymentSucceededEvent(1, 100m, 1, DateTime.UtcNow, "test@example.com")) { NotificationId = 0 };

        var mockRepo = new Mock<INotificationRepository>();
        mockRepo.Setup(r => r.SaveNotification(It.IsAny<Notification>())).ThrowsAsync(new Exception("DB error"));

        var mockLogger = new Mock<ILogger<ServiceType>>();
        var service = new ServiceType(mockRepo.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.SendNotificationAsync(notification));
    }

    [Fact]
    public async Task SendNotification_SetsNotificationId_WhenRepositoryAssignsId()
    {
        // Arrange
        var notification = new Notification(new PaymentSucceededEvent(2, 200m, 2, DateTime.UtcNow, "assign@test.com")) { NotificationId = 0 };

        var mockRepo = new Mock<INotificationRepository>();
        mockRepo.Setup(r => r.SaveNotification(It.IsAny<Notification>())).ReturnsAsync((Notification n) => { n.NotificationId = 5; return n; });

        var mockLogger = new Mock<ILogger<ServiceType>>();
        var service = new ServiceType(mockRepo.Object, mockLogger.Object);

        // Act
        await service.SendNotificationAsync(notification);

        // Assert
        Assert.Equal(5, notification.NotificationId);
        mockRepo.Verify(r => r.SaveNotification(It.IsAny<Notification>()), Times.Once);
    }
}
