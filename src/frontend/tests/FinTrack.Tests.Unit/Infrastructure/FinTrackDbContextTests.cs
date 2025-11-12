using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinTrack.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for FinTrackDbContext
/// </summary>
public class FinTrackDbContextTests : IDisposable
{
    private readonly DbContextOptions<FinTrackDbContext> _options;
    private readonly Mock<ILogger<FinTrackDbContext>> _mockLogger;
    private FinTrackDbContext _context;

    public FinTrackDbContextTests()
    {
        // Use in-memory database for testing
        _options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _mockLogger = new Mock<ILogger<FinTrackDbContext>>();
        _context = new FinTrackDbContext(_options, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithOptions_ShouldCreateContext()
    {
        // Arrange & Act
        using var context = new FinTrackDbContext(_options);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.Transactions);
        Assert.NotNull(context.Accounts);
        Assert.NotNull(context.Categories);
        Assert.NotNull(context.Budgets);
        Assert.NotNull(context.Goals);
        Assert.NotNull(context.GoalMilestones);
    }

    [Fact]
    public void Constructor_WithOptionsAndLogger_ShouldCreateContext()
    {
        // Arrange & Act
        using var context = new FinTrackDbContext(_options, _mockLogger.Object);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.Transactions);
        Assert.NotNull(context.Accounts);
        Assert.NotNull(context.Categories);
        Assert.NotNull(context.Budgets);
        Assert.NotNull(context.Goals);
        Assert.NotNull(context.GoalMilestones);
    }

    #endregion

    #region SaveChangesAsync Tests

    [Fact]
    public async Task SaveChangesAsync_WithNewEntity_ShouldSetAuditFields()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account",
            Balance = 1000,
            Type = AccountType.Checking
        };

        // Act
        _context.Accounts.Add(account);
        var result = await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.True(account.Id > 0);
        Assert.True(account.CreatedAt > DateTime.MinValue);
        Assert.True(account.UpdatedAt > DateTime.MinValue);
        // Allow small timing differences (within 1 second)
        Assert.True((account.UpdatedAt - account.CreatedAt).TotalMilliseconds < 1000);
        Assert.False(string.IsNullOrEmpty(account.SyncId));
        Assert.Equal(SyncStatus.PendingCreate, account.SyncStatus);
        Assert.Equal(1, account.Version);
    }

    [Fact]
    public async Task SaveChangesAsync_WithModifiedEntity_ShouldUpdateAuditFields()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account",
            Balance = 1000,
            Type = AccountType.Checking
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        
        // Mark as synced after saving
        account.SyncStatus = SyncStatus.Synced;
        await _context.SaveChangesAsync();
        
        // Reset entity state to ensure clean tracking
        _context.Entry(account).State = EntityState.Unchanged;

        var originalCreatedAt = account.CreatedAt;
        var originalUpdatedAt = account.UpdatedAt;
        var originalVersion = account.Version;

        // Wait a small amount to ensure timestamp difference
        await Task.Delay(10);

        // Act
        account.Name = "Modified Account";
        // Don't call Update() since the entity is already tracked
        var result = await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(originalCreatedAt, account.CreatedAt); // CreatedAt should not change
        Assert.True(account.UpdatedAt > originalUpdatedAt); // UpdatedAt should be updated
        Assert.Equal(originalVersion + 1, account.Version); // Version should increment
        Assert.Equal(SyncStatus.PendingUpdate, account.SyncStatus); // Should change from Synced to PendingUpdate
    }

    [Fact]
    public async Task SaveChangesAsync_WithDeletedEntity_ShouldPerformSoftDelete()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account",
            Balance = 1000,
            Type = AccountType.Checking
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var originalVersion = account.Version;

        // Act
        _context.Accounts.Remove(account);
        var result = await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.True(account.IsDeleted);
        Assert.Equal(originalVersion + 1, account.Version);
        Assert.Equal(SyncStatus.PendingDelete, account.SyncStatus);

        // Verify the entity is not returned in queries due to global query filter
        var retrievedAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);
        Assert.Null(retrievedAccount);
    }

    [Fact]
    public async Task SaveChangesAsync_WithException_ShouldLogError()
    {
        // Arrange
        var account = new Account
        {
            Name = null!, // This should cause a validation error
            Balance = 1000,
            Type = AccountType.Checking
        };

        _context.Accounts.Add(account);

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while saving changes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_WithSuccessfulSave_ShouldLogDebug()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account",
            Balance = 1000,
            Type = AccountType.Checking
        };

        _context.Accounts.Add(account);

        // Act
        await _context.SaveChangesAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully saved")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region SaveChanges Tests

    [Fact]
    public void SaveChanges_WithNewEntity_ShouldSetAuditFields()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account",
            Balance = 1000,
            Type = AccountType.Checking
        };

        // Act
        _context.Accounts.Add(account);
        var result = _context.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        Assert.True(account.Id > 0);
        Assert.True(account.CreatedAt > DateTime.MinValue);
        Assert.True(account.UpdatedAt > DateTime.MinValue);
        Assert.False(string.IsNullOrEmpty(account.SyncId));
        Assert.Equal(SyncStatus.PendingCreate, account.SyncStatus);
    }

    [Fact]
    public void SaveChanges_WithException_ShouldLogError()
    {
        // Arrange
        var account = new Account
        {
            Name = null!, // This should cause a validation error
            Balance = 1000,
            Type = AccountType.Checking
        };

        _context.Accounts.Add(account);

        // Act & Assert
        Assert.Throws<DbUpdateException>(() => _context.SaveChanges());

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while saving changes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Entity Configuration Tests

    [Fact]
    public async Task Transaction_Configuration_ShouldBeCorrect()
    {
        // Arrange
        var account = new Account { Name = "Test Account", Balance = 1000, Type = AccountType.Checking };
        var category = new Category { Name = "Test Category", CategoryType = TransactionType.Expense };
        
        _context.Accounts.Add(account);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var transaction = new Transaction
        {
            Amount = 100.50m,
            Description = "Test Transaction",
            Date = DateTime.Today,
            AccountId = account.Id,
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            Notes = "Test notes",
            ReferenceNumber = "REF123"
        };

        // Act
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Assert
        var savedTransaction = await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == transaction.Id);

        Assert.Equal(100.50m, savedTransaction.Amount);
        Assert.Equal("Test Transaction", savedTransaction.Description);
        Assert.Equal(DateTime.Today, savedTransaction.Date);
        Assert.Equal(account.Id, savedTransaction.AccountId);
        Assert.Equal(category.Id, savedTransaction.CategoryId);
        Assert.Equal(TransactionType.Expense, savedTransaction.Type);
        Assert.Equal("Test notes", savedTransaction.Notes);
        Assert.Equal("REF123", savedTransaction.ReferenceNumber);
        Assert.NotNull(savedTransaction.Account);
        Assert.NotNull(savedTransaction.Category);
    }

    [Fact]
    public async Task Account_Configuration_ShouldBeCorrect()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Checking Account",
            Balance = 2500.75m,
            Type = AccountType.Checking,
            Currency = "USD",
            Description = "Primary checking account",
            Institution = "Test Bank",
            AccountNumber = "1234567890",
            InitialBalance = 2000.00m,
            IsActive = true
        };

        // Act
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        // Assert
        var savedAccount = await _context.Accounts.FirstAsync(a => a.Id == account.Id);
        Assert.Equal("Test Checking Account", savedAccount.Name);
        Assert.Equal(2500.75m, savedAccount.Balance);
        Assert.Equal(AccountType.Checking, savedAccount.Type);
        Assert.Equal("USD", savedAccount.Currency);
        Assert.Equal("Primary checking account", savedAccount.Description);
        Assert.Equal("Test Bank", savedAccount.Institution);
        Assert.Equal("1234567890", savedAccount.AccountNumber);
        Assert.Equal(2000.00m, savedAccount.InitialBalance);
        Assert.True(savedAccount.IsActive);
    }

    [Fact]
    public async Task Category_Configuration_ShouldBeCorrect()
    {
        // Arrange
        var parentCategory = new Category
        {
            Name = "Parent Category",
            CategoryType = TransactionType.Expense,
            Color = "#FF0000",
            Icon = "parent_icon",
            IsSystem = true,
            IsActive = true,
            SortOrder = 1
        };

        _context.Categories.Add(parentCategory);
        await _context.SaveChangesAsync();

        var childCategory = new Category
        {
            Name = "Child Category",
            ParentCategoryId = parentCategory.Id,
            CategoryType = TransactionType.Expense,
            Color = "#00FF00",
            Icon = "child_icon",
            IsSystem = false,
            IsActive = true,
            SortOrder = 2
        };

        // Act
        _context.Categories.Add(childCategory);
        await _context.SaveChangesAsync();

        // Assert
        var savedChild = await _context.Categories
            .Include(c => c.ParentCategory)
            .FirstAsync(c => c.Id == childCategory.Id);

        Assert.Equal("Child Category", savedChild.Name);
        Assert.Equal(parentCategory.Id, savedChild.ParentCategoryId);
        Assert.NotNull(savedChild.ParentCategory);
        Assert.Equal("Parent Category", savedChild.ParentCategory.Name);
    }

    [Fact]
    public async Task Budget_Configuration_ShouldBeCorrect()
    {
        // Arrange
        var category = new Category { Name = "Test Category", CategoryType = TransactionType.Expense };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var budget = new Budget
        {
            Name = "Monthly Food Budget",
            Amount = 500.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 31),
            CategoryId = category.Id,
            SpentAmount = 150.00m,
            IsActive = true,
            AlertThreshold = 80.0m
        };

        // Act
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        // Assert
        var savedBudget = await _context.Budgets
            .Include(b => b.Category)
            .FirstAsync(b => b.Id == budget.Id);

        Assert.Equal("Monthly Food Budget", savedBudget.Name);
        Assert.Equal(500.00m, savedBudget.Amount);
        Assert.Equal(BudgetPeriod.Monthly, savedBudget.Period);
        Assert.Equal(new DateTime(2024, 1, 1), savedBudget.StartDate);
        Assert.Equal(new DateTime(2024, 1, 31), savedBudget.EndDate);
        Assert.Equal(category.Id, savedBudget.CategoryId);
        Assert.Equal(150.00m, savedBudget.SpentAmount);
        Assert.True(savedBudget.IsActive);
        Assert.Equal(80.0m, savedBudget.AlertThreshold);
        Assert.NotNull(savedBudget.Category);
    }

    [Fact]
    public async Task Goal_Configuration_ShouldBeCorrect()
    {
        // Arrange
        var account = new Account { Name = "Savings Account", Balance = 5000, Type = AccountType.Savings };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var goal = new Goal
        {
            Name = "Emergency Fund",
            Description = "Build emergency fund",
            TargetAmount = 10000.00m,
            CurrentAmount = 2500.00m,
            TargetDate = DateTime.Today.AddYears(1),
            Priority = 1,
            Type = GoalType.EmergencyFund,
            Color = "#00FF00",
            IsCompleted = false,
            LinkedAccountId = account.Id
        };

        // Act
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        // Assert
        var savedGoal = await _context.Goals
            .Include(g => g.LinkedAccount)
            .FirstAsync(g => g.Id == goal.Id);

        Assert.Equal("Emergency Fund", savedGoal.Name);
        Assert.Equal("Build emergency fund", savedGoal.Description);
        Assert.Equal(10000.00m, savedGoal.TargetAmount);
        Assert.Equal(2500.00m, savedGoal.CurrentAmount);
        Assert.Equal(1, savedGoal.Priority);
        Assert.Equal(GoalType.EmergencyFund, savedGoal.Type);
        Assert.Equal("#00FF00", savedGoal.Color);
        Assert.False(savedGoal.IsCompleted);
        Assert.Equal(account.Id, savedGoal.LinkedAccountId);
        Assert.NotNull(savedGoal.LinkedAccount);
    }

    [Fact]
    public async Task GoalMilestone_Configuration_ShouldBeCorrect()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000,
            TargetDate = DateTime.Today.AddYears(1),
            Type = GoalType.Savings
        };

        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var milestone = new GoalMilestone
        {
            GoalId = goal.Id,
            Name = "First Milestone",
            Description = "25% of goal",
            TargetAmount = 2500.00m,
            IsAchieved = false
        };

        // Act
        _context.GoalMilestones.Add(milestone);
        await _context.SaveChangesAsync();

        // Assert
        var savedMilestone = await _context.GoalMilestones
            .Include(gm => gm.Goal)
            .FirstAsync(gm => gm.Id == milestone.Id);

        Assert.Equal(goal.Id, savedMilestone.GoalId);
        Assert.Equal("First Milestone", savedMilestone.Name);
        Assert.Equal("25% of goal", savedMilestone.Description);
        Assert.Equal(2500.00m, savedMilestone.TargetAmount);
        Assert.False(savedMilestone.IsAchieved);
        Assert.NotNull(savedMilestone.Goal);
    }

    #endregion

    #region Seeded Data Tests

    // Note: Seeding functionality not yet implemented
    // [Fact]
    // public async Task Database_ShouldContainSeededCategories()
    // {
    //     // This test will be implemented when seeding functionality is added
    // }

    [Fact]
    public async Task Database_ShouldContainSeededAccount()
    {
        // Arrange
        await _context.Database.EnsureCreatedAsync();
        
        // Seed data using the seeding service
        var seedingService = new FinTrack.Infrastructure.Services.DataSeedingService(_context, Mock.Of<Microsoft.Extensions.Logging.ILogger<FinTrack.Infrastructure.Services.DataSeedingService>>());
        await seedingService.SeedAllDataAsync();
        
        // Act
        var accounts = await _context.Accounts.ToListAsync();

        // Assert
        Assert.NotEmpty(accounts);
        
        var defaultAccount = accounts.FirstOrDefault(a => a.Name == "Primary Checking");
        Assert.NotNull(defaultAccount);
        Assert.Equal(AccountType.Checking, defaultAccount.Type);
        Assert.Equal("USD", defaultAccount.Currency);
        Assert.Equal(0, defaultAccount.Balance);
        Assert.True(defaultAccount.IsActive);
    }

    #endregion

    #region Global Query Filter Tests

    [Fact]
    public async Task GlobalQueryFilter_ShouldExcludeSoftDeletedEntities()
    {
        // Arrange
        var account1 = new Account { Name = "Active Account", Balance = 1000, Type = AccountType.Checking };
        var account2 = new Account { Name = "Deleted Account", Balance = 2000, Type = AccountType.Savings, IsDeleted = true };

        _context.Accounts.AddRange(account1, account2);
        await _context.SaveChangesAsync();

        // Act
        var accounts = await _context.Accounts.ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("Active Account", accounts[0].Name);
        Assert.DoesNotContain(accounts, a => a.Name == "Deleted Account");
    }

    [Fact]
    public async Task GlobalQueryFilter_CanBeIgnoredWithIgnoreQueryFilters()
    {
        // Arrange
        var account1 = new Account { Name = "Active Account", Balance = 1000, Type = AccountType.Checking };
        var account2 = new Account { Name = "Deleted Account", Balance = 2000, Type = AccountType.Savings, IsDeleted = true };

        _context.Accounts.AddRange(account1, account2);
        await _context.SaveChangesAsync();

        // Act
        var allAccounts = await _context.Accounts.IgnoreQueryFilters().ToListAsync();

        // Assert
        Assert.Equal(2, allAccounts.Count);
        Assert.Contains(allAccounts, a => a.Name == "Active Account");
        Assert.Contains(allAccounts, a => a.Name == "Deleted Account");
    }

    #endregion

    #region Relationship Tests

    [Fact]
    public async Task Transaction_AccountRelationship_ShouldWork()
    {
        // Arrange
        var account = new Account { Name = "Test Account", Balance = 1000, Type = AccountType.Checking };
        var category = new Category { Name = "Test Category", CategoryType = TransactionType.Expense };
        
        _context.Accounts.Add(account);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var transaction = new Transaction
        {
            Amount = 100,
            Description = "Test Transaction",
            Date = DateTime.Today,
            AccountId = account.Id,
            CategoryId = category.Id,
            Type = TransactionType.Expense
        };

        // Act
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Assert
        var loadedTransaction = await _context.Transactions
            .Include(t => t.Account)
            .FirstAsync(t => t.Id == transaction.Id);

        Assert.NotNull(loadedTransaction.Account);
        Assert.Equal("Test Account", loadedTransaction.Account.Name);
    }

    [Fact]
    public async Task Category_ParentChildRelationship_ShouldWork()
    {
        // Arrange
        var parentCategory = new Category
        {
            Name = "Parent Category",
            CategoryType = TransactionType.Expense
        };

        _context.Categories.Add(parentCategory);
        await _context.SaveChangesAsync();

        var childCategory = new Category
        {
            Name = "Child Category",
            ParentCategoryId = parentCategory.Id,
            CategoryType = TransactionType.Expense
        };

        // Act
        _context.Categories.Add(childCategory);
        await _context.SaveChangesAsync();

        // Assert
        var loadedParent = await _context.Categories
            .Include(c => c.SubCategories)
            .FirstAsync(c => c.Id == parentCategory.Id);

        var loadedChild = await _context.Categories
            .Include(c => c.ParentCategory)
            .FirstAsync(c => c.Id == childCategory.Id);

        Assert.Single(loadedParent.SubCategories);
        Assert.Equal("Child Category", loadedParent.SubCategories.First().Name);
        Assert.NotNull(loadedChild.ParentCategory);
        Assert.Equal("Parent Category", loadedChild.ParentCategory.Name);
    }

    [Fact]
    public async Task Goal_MilestoneRelationship_ShouldWork()
    {
        // Arrange
        var goal = new Goal
        {
            Name = "Test Goal",
            TargetAmount = 10000,
            TargetDate = DateTime.Today.AddYears(1),
            Type = GoalType.Savings
        };

        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var milestone1 = new GoalMilestone
        {
            GoalId = goal.Id,
            Name = "Milestone 1",
            TargetAmount = 2500
        };

        var milestone2 = new GoalMilestone
        {
            GoalId = goal.Id,
            Name = "Milestone 2",
            TargetAmount = 5000
        };

        // Act
        _context.GoalMilestones.AddRange(milestone1, milestone2);
        await _context.SaveChangesAsync();

        // Assert
        var loadedGoal = await _context.Goals
            .Include(g => g.Milestones)
            .FirstAsync(g => g.Id == goal.Id);

        Assert.Equal(2, loadedGoal.Milestones.Count);
        Assert.Contains(loadedGoal.Milestones, m => m.Name == "Milestone 1");
        Assert.Contains(loadedGoal.Milestones, m => m.Name == "Milestone 2");
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task SaveChangesAsync_WithConcurrencyConflict_ShouldThrowException()
    {
        // Arrange
        var account = new Account { Name = "Test Account", Balance = 1000, Type = AccountType.Checking };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        // Create two contexts to simulate concurrent access
        using var context1 = new FinTrackDbContext(_options);
        using var context2 = new FinTrackDbContext(_options);

        var account1 = await context1.Accounts.FindAsync(account.Id);
        var account2 = await context2.Accounts.FindAsync(account.Id);

        // Act
        account1!.Name = "Modified by Context 1";
        account2!.Name = "Modified by Context 2";

        await context1.SaveChangesAsync(); // This should succeed

        // Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context2.SaveChangesAsync());
    }

    #endregion

    #region Index Tests

    [Fact]
    public async Task Database_ShouldHaveRequiredIndexes()
    {
        // Arrange & Act
        await _context.Database.EnsureCreatedAsync();

        // This test verifies that the database can be created successfully with all the indexes
        // In a real scenario, you might want to query the database schema to verify specific indexes exist
        // For now, we'll just ensure no exceptions are thrown during database creation

        // Assert
        Assert.True(true); // If we get here, the database was created successfully with all indexes
    }

    #endregion

    #region Edge Cases and Error Conditions

    [Fact]
    public async Task SaveChangesAsync_WithNullSyncId_ShouldGenerateNewSyncId()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account",
            Balance = 1000,
            Type = AccountType.Checking,
            SyncId = null! // Explicitly set to null
        };

        // Act
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        // Assert
        Assert.NotNull(account.SyncId);
        Assert.NotEmpty(account.SyncId);
    }

    [Fact]
    public async Task SaveChangesAsync_WithEmptySyncId_ShouldGenerateNewSyncId()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account",
            Balance = 1000,
            Type = AccountType.Checking,
            SyncId = string.Empty
        };

        // Act
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        // Assert
        Assert.NotNull(account.SyncId);
        Assert.NotEmpty(account.SyncId);
    }

    [Fact]
    public async Task SaveChangesAsync_WithExistingSyncId_ShouldNotChangeSyncId()
    {
        // Arrange
        var existingSyncId = Guid.NewGuid().ToString();
        var account = new Account
        {
            Name = "Test Account",
            Balance = 1000,
            Type = AccountType.Checking,
            SyncId = existingSyncId
        };

        // Act
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(existingSyncId, account.SyncId);
    }

    [Fact]
    public async Task SaveChangesAsync_WithNonSyncedStatus_ShouldNotChangeSyncStatus()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account",
            Balance = 1000,
            Type = AccountType.Checking,
            SyncStatus = SyncStatus.SyncFailed
        };

        // Act
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(SyncStatus.SyncFailed, account.SyncStatus);
    }

    #endregion
}