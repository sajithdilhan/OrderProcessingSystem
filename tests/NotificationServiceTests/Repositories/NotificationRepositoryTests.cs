using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NotificationServiceTests.Repositories;

public class NotificationRepositoryTests
{
    private NotificationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new NotificationDbContext(options);
    }

    [Fact]
    public async Task GetAllNotifications_ReturnsEmpty_WhenNoNotifications()
    {
        // Arrange
        var dbName = $"notifications_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new NotificationRepository(context);

        // Act
        var list = await repo.GetAllNotifications();

        // Assert
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public async Task SaveNotification_PersistsNotificationAndAssignsId()
    {
        // Arrange
        var dbName = $"notifications_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new NotificationRepository(context);

        var notification = new Notification("msg");

        // Act
        var created = await repo.SaveNotification(notification);

        // Assert
        Assert.NotNull(created);
        Assert.True(created.NotificationId > 0);

        var fromDb = context.Notifications.ToList();
        Assert.Single(fromDb);
        Assert.Equal(created.NotificationId, fromDb[0].NotificationId);
    }

    [Fact]
    public void CreatingNotification_WithEmptyMessage_Allows()
    {
        // Arrange & Act
        var n = new Notification("");
        Assert.Equal(string.Empty, n.Message);
    }
}
