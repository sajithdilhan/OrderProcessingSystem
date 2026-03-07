using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Api.Controllers;
using NotificationService.Application.Dto;
using NotificationService.Application.Interfaces;
using Shared.Contracts.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace NotificationServiceTests.Controllers;

public class NotificationsControllerTests
{
    [Fact]
    public async Task GetNotifications_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var notifications = new List<NotificationResponse>
        {
            new NotificationResponse { NotificationId = 1, Message = "Hello" }
        };

        var mockService = new Mock<INotificationService>();
        mockService.Setup(s => s.GetAllNotificationsAsync()).ReturnsAsync(Result<IEnumerable<NotificationResponse>>.Success(notifications));

        var mockLogger = new Mock<ILogger<NotificationsController>>();
        var controller = new NotificationsController(mockService.Object, mockLogger.Object);

        // Act
        var result = await controller.GetNotifications();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(notifications, ok.Value);
    }

    [Fact]
    public async Task GetNotifications_ReturnsProblem_WhenServiceFails()
    {
        // Arrange
        var error = new Error(404, "Not found");
        var mockService = new Mock<INotificationService>();
        mockService.Setup(s => s.GetAllNotificationsAsync()).ReturnsAsync(Result<IEnumerable<NotificationResponse>>.Failure(error));

        var mockLogger = new Mock<ILogger<NotificationsController>>();
        var controller = new NotificationsController(mockService.Object, mockLogger.Object);

        // Act
        var result = await controller.GetNotifications();

        // Assert
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, obj.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(obj.Value);
        Assert.Equal("Not found", problem.Detail);
    }
}
