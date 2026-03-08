using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Validations;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.Repositories;
using Shared.Contracts.Enum;

namespace PaymentServiceTests.Repositories;

public class PaymentRepositoryTests
{
    private PaymentsDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new PaymentsDbContext(options);
    }

    [Fact]
    public async Task GetAll_ReturnsEmpty_WhenNoPayments()
    {
        // Arrange
        var dbName = $"payments_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new PaymentRepository(context);

        // Act
        var list = await repo.GetAllPaymentsAsync();

        // Assert
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public async Task SavePaymentAsync_PersistsPaymentAndAssignsId()
    {
        // Arrange
        var dbName = $"payments_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new PaymentRepository(context);

        var payment = new Payment(25m, 6, "test@test.com", paymentStatus: PaymentStatus.Pending);

        // Act
        var created = await repo.SavePaymentAsync(payment);

        // Assert
        Assert.NotNull(created);
        Assert.True(created.PaymentId > 0);

        var fromDb = context.Payments.ToList();
        Assert.Single(fromDb);
        Assert.Equal(created.PaymentId, fromDb[0].PaymentId);
    }

    [Fact]
    public async Task UpdatePaymentAsync_UpdatesExistingPayment()
    {
        // Arrange
        var dbName = $"payments_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new PaymentRepository(context);

        var payment = new Payment(40m, 7, "test@test.com", paymentStatus: PaymentStatus.Pending);
        var created = await repo.SavePaymentAsync(payment);

        // Act
        created.PaymentStatus = PaymentStatus.Completed;
        var updated = await repo.UpdatePaymentAsync(created);

        // Assert
        Assert.Equal(PaymentStatus.Completed, updated.PaymentStatus);

        var fromDb = context.Payments.ToList();
        Assert.Single(fromDb);
        Assert.Equal(PaymentStatus.Completed, fromDb[0].PaymentStatus);
    }

    [Fact]
    public void CreatingPayment_WithInvalidArgs_ThrowsValidation()
    {
        // Arrange & Act & Assert
        Assert.Throws<PaymentValidationException>(() => new Payment(0, 1, "test@test.com", paymentStatus: PaymentStatus.Pending));
        Assert.Throws<PaymentValidationException>(() => new Payment(10, 0, "test@test.com", paymentStatus: PaymentStatus.Pending));
    }

    [Fact]
    public void CreatingPayment_WithNegativeArgs_ThrowsValidation()
    {
        // Arrange & Act & Assert
        Assert.Throws<PaymentValidationException>(() => new Payment(-1, 1, "test@test.com", paymentStatus: PaymentStatus.Pending));
        Assert.Throws<PaymentValidationException>(() => new Payment(10, -2, "test@test.com", paymentStatus: PaymentStatus.Pending));
    }

    [Fact]
    public async Task SavePayment_SetsPaymentDateToNow()
    {
        // Arrange
        var dbName = $"payments_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new PaymentRepository(context);

        var before = DateTime.UtcNow;
        var payment = new Payment(25m, 6, "time@test.com", paymentStatus: PaymentStatus.Pending);

        // Act
        var created = await repo.SavePaymentAsync(payment);
        var after = DateTime.UtcNow;

        // Assert
        Assert.True(created.PaymentDate >= before && created.PaymentDate <= after);
    }

    [Fact]
    public async Task SavePayment_PersistsMultiplePayments()
    {
        // Arrange
        var dbName = $"payments_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new PaymentRepository(context);

        var p1 = new Payment(10m, 1, "test1@test.com", paymentStatus: PaymentStatus.Pending);
        var p2 = new Payment(20m, 2, "test2@test.com", paymentStatus: PaymentStatus.Pending);

        // Act
        var created1 = await repo.SavePaymentAsync(p1);
        var created2 = await repo.SavePaymentAsync(p2);

        // Assert
        var fromDb = context.Payments.ToList();
        Assert.Equal(2, fromDb.Count);
        Assert.Contains(fromDb, o => o.PaymentId == created1.PaymentId);
        Assert.Contains(fromDb, o => o.PaymentId == created2.PaymentId);
    }

    [Fact]
    public async Task GetPaymentByOrderId_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var dbName = $"payments_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new PaymentRepository(context);

        // Act
        var result = await repo.GetPaymentByOrderIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPaymentByOrderId_ReturnsPayment_WhenExists()
    {
        // Arrange
        var dbName = $"payments_{Guid.NewGuid()}";
        await using var context = CreateContext(dbName);
        var repo = new PaymentRepository(context);
        var payment = new Payment(50m, 10, "test@test.com", paymentStatus: PaymentStatus.Pending);
        await repo.SavePaymentAsync(payment);

        // Act
        var result = await repo.GetPaymentByOrderIdAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.OrderId);
        Assert.Equal(50m, result.Amount);
    }
}
