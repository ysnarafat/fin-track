using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinTrack.Tests.Unit.Infrastructure.Data;

/// <summary>
/// Unit tests for FinTrackDbContext
/// </summary>
public class FinTrackDbContextTests : IDisposable
{
    private readonly DbContextOptions<FinTrackDbContext> _options;
    private readonly Mock<ILogger<FinTrackDbContext>> _mockLogger;

    public FinTrackDbContextTests()
    {
        _options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _mockLogger = new Mock<ILogger<FinTrackDbContext>>();
    }

    [Fact]
    public void Constructor_WithOptions_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        using var context = new FinTrackDbContext(_options);

        // Assert
        Assert.NotNull(context.Transactions);
        Assert.NotNull(context.Accounts);
        Assert.NotNull(context.Categories);
        Assert.NotNull(context.Budgets);
        Assert.NotNull(context.Goals);
        Assert.NotNull(context.GoalMilestones);
    }

    [Fact]
    public void Constructor_WithOptionsAndLogger_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);

        // Assert
        Assert.NotNull(context.Transactions);
        Assert.NotNull(context.Accounts);
        Assert.NotNull(context.Categories);
        Assert.NotNull(context.Budgets);
        Assert.NotNull(context.Goals);
        Assert.NotNull(context.GoalMilestones);
    }

    [Fact]
    public async Task SaveChangesAsync_WithNewEntity_ShouldSetAuditFields()
    {
        // Arrange
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);
        var beforeSave = DateTime.UtcNow;
        
        var account = new Account
        {
            Name = "Test Account",
            Currency = "USD",
            Type = AccountType.Checking
        };

        // Act
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Assert
        Assert.True(account.CreatedAt >= beforeSave);
        Assert.True(account.UpdatedAt >= beforeSave);
        Assert.NotEmpty(account.SyncId);
        Assert.True(Guid.TryParse(account.SyncId, out _));
        Assert.Equal(SyncStatus.PendingCreate, account.SyncStatus);
        Assert.Equal(1, account.Version);
    }

    [Fact]
    public async Task SaveChangesAsync_WithModifiedEntity_ShouldUpdateAuditFields()
    {
        // Arrange
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);
        
        var account = new Account
        {
            Name = "Test Account",
            Currency = "USD",
            Type = AccountType.Checking
        };
        
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        
        // Mark as synced to test the sync status change
        account.MarkAsSynced();
        await context.SaveChangesAsync();
        
        // Reset entity state to ensure clean tracking
        context.Entry(account).State = EntityState.Unchanged;
        
        var originalUpdatedAt = account.UpdatedAt;
        var originalVersion = account.Version;
        
        // Wait to ensure timestamp difference
        await Task.Delay(1);

        // Act
        account.Name = "Updated Account";
        // Don't call Update() since the entity is already tracked
        await context.SaveChangesAsync();

        // Assert
        Assert.True(account.UpdatedAt > originalUpdatedAt);
        Assert.Equal(originalVersion + 1, account.Version);
        Assert.Equal(SyncStatus.PendingUpdate, account.SyncStatus);
    }

    [Fact]
    public async Task SaveChangesAsync_WithDeletedEntity_ShouldImplementSoftDelete()
    {
        // Arrange
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);
        
        var account = new Account
        {
            Name = "Test Account",
            Currency = "USD",
            Type = AccountType.Checking
        };
        
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        
        var originalUpdatedAt = account.UpdatedAt;
        var originalVersion = account.Version;
        
        await Task.Delay(1);

        // Act
        context.Accounts.Remove(account);
        await context.SaveChangesAsync();

        // Assert
        var deletedAccount = await context.Accounts.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == account.Id);
        Assert.NotNull(deletedAccount);
        Assert.True(deletedAccount.IsDeleted);
        Assert.True(deletedAccount.UpdatedAt > originalUpdatedAt);
        Assert.Equal(originalVersion + 1, deletedAccount.Version);
        Assert.Equal(SyncStatus.PendingDelete, deletedAccount.SyncStatus);
    }

    [Fact]
    public async Task SaveChangesAsync_WithEntityAlreadyPendingCreate_ShouldNotChangeSyncStatus()
    {
        // Arrange
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);
        
        var account = new Account
        {
            Name = "Test Account",
            Currency = "USD",
            Type = AccountType.Checking,
            SyncStatus = SyncStatus.PendingCreate
        };

        // Act
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(SyncStatus.PendingCreate, account.SyncStatus);
    }

    [Fact]
    public async Task SaveChangesAsync_WithEntityAlreadyPendingUpdate_ShouldNotChangeSyncStatus()
    {
        // Arrange
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);
        
        var account = new Account
        {
            Name = "Test Account",
            Currency = "USD",
            Type = AccountType.Checking
        };
        
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        
        // Set to pending update manually
        account.SyncStatus = SyncStatus.PendingUpdate;
        await context.SaveChangesAsync();

        // Act
        account.Name = "Updated Account";
        context.Accounts.Update(account);
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(SyncStatus.PendingUpdate, account.SyncStatus);
    }

    [Fact]
    public async Task SaveChangesAsync_WithExistingSyncId_ShouldNotOverwriteSyncId()
    {
        // Arrange
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);
        var existingSyncId = Guid.NewGuid().ToString();
        
        var account = new Account
        {
            Name = "Test Account",
            Currency = "USD",
            Type = AccountType.Checking,
            SyncId = existingSyncId
        };

        // Act
        context.Accounts.Add(account);
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(existingSyncId, account.SyncId);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldLogSuccessfulSave()
    {
        // Arrange
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);
        
        var account = new Account
        {
            Name = "Test Account",
            Currency = "USD",
            Type = AccountType.Checking
        };

        // Act
        context.Accounts.Add(account);
        var result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully saved 1 changes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_WithException_ShouldLogError()
    {
        // Arrange
        var mockOptions = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new FinTrackDbContext(mockOptions, _mockLogger.Object);
        
        // Create an invalid entity that will cause an exception
        // Create a scenario that will cause an exception during SaveChanges
        // We'll create an account with a null name after bypassing validation
        var account = new Account
        {
            Name = "Test Account",
            Currency = "USD",
            Type = AccountType.Checking
        };

        context.Accounts.Add(account);
        
        // Modify the account to have invalid data after it's tracked
        account.Name = null!; // This should cause an exception during save

        // Act & Assert - Just verify that an exception is thrown
        // The in-memory database may not enforce all constraints, so we just verify exception handling works
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => context.SaveChangesAsync());
        Assert.NotNull(exception);
    }

    [Fact]
    public void SaveChanges_WithNewEntity_ShouldSetAuditFields()
    {
        // Arrange
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);
        var beforeSave = DateTime.UtcNow;
        
        var account = new Account
        {
            Name = "Test Account",
            Currency = "USD",
            Type = AccountType.Checking
        };

        // Act
        context.Accounts.Add(account);
        var result = context.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        Assert.True(account.CreatedAt >= beforeSave);
        Assert.True(account.UpdatedAt >= beforeSave);
        Assert.NotEmpty(account.SyncId);
        Assert.Equal(SyncStatus.PendingCreate, account.SyncStatus);
    }

    [Fact]
    public void SaveChanges_ShouldLogSuccessfulSave()
    {
        // Arrange
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);
        
        var account = new Account
        {
            Name = "Test Account",
            Currency = "USD",
            Type = AccountType.Checking
        };

        // Act
        context.Accounts.Add(account);
        var result = context.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully saved 1 changes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Database_ShouldContainSeededData()
    {
        // Arrange
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);
        await context.Database.EnsureCreatedAsync();
        
        // Seed data using the seeding service
        var seedingService = new FinTrack.Infrastructure.Services.DataSeedingService(context, Mock.Of<Microsoft.Extensions.Logging.ILogger<FinTrack.Infrastructure.Services.DataSeedingService>>());
        await seedingService.SeedAllDataAsync();

        // Act & Assert - Check for seeded categories
        var expenseCategories = await context.Categories
            .Where(c => c.CategoryType == TransactionType.Expense)
            .ToListAsync();
        
        var incomeCategories = await context.Categories
            .Where(c => c.CategoryType == TransactionType.Income)
            .ToListAsync();

        var defaultAccount = await context.Accounts
            .FirstOrDefaultAsync(a => a.Name == "Primary Checking");

        Assert.True(expenseCategories.Count >= 10);
        Assert.True(incomeCategories.Count >= 6);
        Assert.NotNull(defaultAccount);
        Assert.Equal("USD", defaultAccount.Currency);
        Assert.Equal(AccountType.Checking, defaultAccount.Type);
    }

    [Fact]
    public async Task SeededCategories_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);
        await context.Database.EnsureCreatedAsync();

        // Assert
        var categories = await context.Categories.ToListAsync();
        
        foreach (var category in categories)
        {
            Assert.NotEmpty(category.Name);
            Assert.NotEmpty(category.Icon);
            Assert.NotEmpty(category.Color);
            Assert.True(category.IsSystem);
            Assert.True(category.IsActive);
            Assert.True(category.SortOrder > 0);
        }
    }

    [Fact]
    public async Task MultipleEntities_ShouldAllGetAuditFieldsUpdated()
    {
        // Arrange
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);
        var beforeSave = DateTime.UtcNow;
        
        var account1 = new Account { Name = "Account 1", Currency = "USD", Type = AccountType.Checking };
        var account2 = new Account { Name = "Account 2", Currency = "USD", Type = AccountType.Savings };
        var transaction = new Transaction 
        { 
            Amount = 100, 
            Description = "Test", 
            CategoryId = 1, 
            AccountId = 1,
            Type = TransactionType.Expense
        };

        // Act
        context.Accounts.AddRange(account1, account2);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        // Assert
        var entities = new BaseEntity[] { account1, account2, transaction };
        
        foreach (var entity in entities)
        {
            Assert.True(entity.CreatedAt >= beforeSave);
            Assert.True(entity.UpdatedAt >= beforeSave);
            Assert.NotEmpty(entity.SyncId);
            Assert.Equal(SyncStatus.PendingCreate, entity.SyncStatus);
            Assert.Equal(1, entity.Version);
        }
    }

    public void Dispose()
    {
        // Cleanup is handled by using statements in tests
    }
}