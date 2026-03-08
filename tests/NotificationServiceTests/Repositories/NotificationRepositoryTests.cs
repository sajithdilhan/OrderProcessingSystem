using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Repositories;
using Shared.Contracts.Events;

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

        var notification = new Notification(new PaymentSucceededEvent(1, 100m, 1, DateTime.UtcNow, "test@example.com"));

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
    public async Task SaveNotification_SetsCreatedAtToNow()
    {
        // Arrange
        var dbName = $"notifications_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new NotificationRepository(context);

        var before = DateTime.UtcNow;
        var n = new Notification(new PaymentSucceededEvent(1, 50m, 1, DateTime.UtcNow, "time@test.com"));

        // Act
        var created = await repo.SaveNotification(n);
        var after = DateTime.UtcNow;

        // Assert
        Assert.True(created.CreatedAt >= before && created.CreatedAt <= after);
    }

    [Fact]
    public async Task SaveNotification_PersistsMultipleNotifications()
    {
        // Arrange
        var dbName = $"notifications_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new NotificationRepository(context);

        var n1 = new Notification(new PaymentSucceededEvent(1, 10m, 1, DateTime.UtcNow, "test1@test.com"));
        var n2 = new Notification(new PaymentSucceededEvent(2, 20m, 2, DateTime.UtcNow, "test2@test.com"));

        // Act
        var created1 = await repo.SaveNotification(n1);
        var created2 = await repo.SaveNotification(n2);

        // Assert
        var fromDb = context.Notifications.ToList();
        Assert.Equal(2, fromDb.Count);
        Assert.Contains(fromDb, o => o.NotificationId == created1.NotificationId);
        Assert.Contains(fromDb, o => o.NotificationId == created2.NotificationId);
    }

    [Fact]
    public async Task GetNotificationById_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var dbName = $"notifications_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new NotificationRepository(context);

        // Act
        var result = await repo.GetNotificationById(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetNotificationById_ReturnsNotification_WhenExists()
    {
        // Arrange
        var dbName = $"notifications_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new NotificationRepository(context);
        var notification = new Notification(new PaymentSucceededEvent(5, 100m, 10, DateTime.UtcNow, "test@test.com"));
        await repo.SaveNotification(notification);

        // Act
        var result = await repo.GetNotificationById(5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Message.PaymentId);
        Assert.Equal(100m, result.Message.Amount);
    }
}
