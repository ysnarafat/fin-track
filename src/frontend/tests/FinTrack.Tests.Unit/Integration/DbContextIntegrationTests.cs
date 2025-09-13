using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Infrastructure.Data;
using FinTrack.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FinTrack.Tests.Unit.Integration;

/// <summary>
/// Integration tests for database operations with FinTrackDbContext
/// </summary>
[Collection("Database Tests")]
public class DbContextIntegrationTests : DatabaseTestBase
{
    [Fact]
    public async Task SaveChangesAsync_WithNewAccount_ShouldPersistToDatabase()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Integration Test Account", 1500m);

        // Act
        Context.Accounts.Add(account);
        var result = await Context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.True(account.Id > 0);
        
        // Verify it was actually saved
        var savedAccount = await Context.Accounts.FindAsync(account.Id);
        Assert.NotNull(savedAccount);
        Assert.Equal("Integration Test Account", savedAccount.Name);
        Assert.Equal(1500m, savedAccount.Balance);
    }

    [Fact]
    public async Task SaveChangesAsync_WithTransaction_ShouldUpdateAccountAndTransaction()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var account = TestDataBuilder.CreateAccount("Test Account", 1000m);
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        var transaction = TestDataBuilder.CreateTransaction(
            amount: 250m,
            description: "Integration Test Transaction",
            accountId: account.Id,
            categoryId: 1 // Should exist from seeded data
        );

        // Act
        Context.Transactions.Add(transaction);
        var result = await Context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.True(transaction.Id > 0);
        
        // Verify transaction was saved with correct audit fields
        var savedTransaction = await Context.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);
        
        Assert.NotNull(savedTransaction);
        Assert.Equal("Integration Test Transaction", savedTransaction.Description);
        Assert.Equal(250m, savedTransaction.Amount);
        Assert.Equal(account.Id, savedTransaction.AccountId);
        Assert.NotNull(savedTransaction.Account);
        Assert.Equal(SyncStatus.PendingCreate, savedTransaction.SyncStatus);
        Assert.NotEmpty(savedTransaction.SyncId);
    }

    [Fact]
    public async Task Query_WithIncludes_ShouldLoadRelatedData()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var account = TestDataBuilder.CreateAccount("Query Test Account");
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        var transactions = TestDataBuilder.CreateTransactionList(3, account.Id);
        Context.Transactions.AddRange(transactions);
        await Context.SaveChangesAsync();

        // Act
        var accountWithTransactions = await Context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == account.Id);

        // Assert
        Assert.NotNull(accountWithTransactions);
        Assert.Equal(3, accountWithTransactions.Transactions.Count);
        
        foreach (var transaction in accountWithTransactions.Transactions)
        {
            Assert.Equal(account.Id, transaction.AccountId);
            Assert.False(transaction.IsDeleted);
        }
    }

    [Fact]
    public async Task SoftDelete_ShouldMarkAsDeletedNotRemove()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Delete Test Account");
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();
        var accountId = account.Id;

        // Act
        Context.Accounts.Remove(account);
        await Context.SaveChangesAsync();

        // Assert
        // Should still exist in database but marked as deleted
        var deletedAccount = await Context.Accounts
            .IgnoreQueryFilters() // Include soft-deleted entities
            .FirstOrDefaultAsync(a => a.Id == accountId);
        
        Assert.NotNull(deletedAccount);
        Assert.True(deletedAccount.IsDeleted);
        Assert.Equal(SyncStatus.PendingDelete, deletedAccount.SyncStatus);

        // Should not appear in normal queries
        var normalQuery = await Context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId);
        Assert.Null(normalQuery);
    }

    [Fact]
    public async Task SeededData_ShouldBeAvailable()
    {
        // Arrange
        await Context.Database.EnsureCreatedAsync();
        
        // Seed data using the seeding service
        var seedingService = new FinTrack.Infrastructure.Services.DataSeedingService(Context, Mock.Of<Microsoft.Extensions.Logging.ILogger<FinTrack.Infrastructure.Services.DataSeedingService>>());
        await seedingService.SeedAllDataAsync();

        // Act & Assert
        var categories = await Context.Categories.ToListAsync();
        var defaultAccount = await Context.Accounts
            .FirstOrDefaultAsync(a => a.Name == "Primary Checking");

        Assert.True(categories.Count >= 16); // At least 10 expense + 6 income categories
        Assert.NotNull(defaultAccount);
        Assert.Equal(AccountType.Checking, defaultAccount.Type);
        Assert.Equal("USD", defaultAccount.Currency);

        // Verify category types
        var expenseCategories = categories.Where(c => c.CategoryType == TransactionType.Expense).ToList();
        var incomeCategories = categories.Where(c => c.CategoryType == TransactionType.Income).ToList();
        
        Assert.True(expenseCategories.Count >= 10);
        Assert.True(incomeCategories.Count >= 6);
        
        // All seeded categories should be system categories
        Assert.All(categories, c => Assert.True(c.IsSystem));
        Assert.All(categories, c => Assert.True(c.IsActive));
    }

    [Fact]
    public async Task ConcurrentUpdates_ShouldHandleVersioning()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Concurrency Test", 1000m);
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        // For in-memory database, we can't really test true concurrency
        // but we can test version handling
        var originalVersion = account.Version;

        // Act - Modify the account
        account.Balance = 1500m;
        await Context.SaveChangesAsync();

        // Assert
        var finalAccount = await Context.Accounts.FindAsync(account.Id);
        Assert.NotNull(finalAccount);
        Assert.Equal(1500m, finalAccount.Balance);
        Assert.Equal(originalVersion + 1, finalAccount.Version);
    }

    [Fact]
    public async Task ComplexQuery_WithMultipleIncludes_ShouldWork()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var account = TestDataBuilder.CreateAccount("Complex Query Account");
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        var transactions = TestDataBuilder.CreateTransactionList(5, account.Id);
        Context.Transactions.AddRange(transactions);
        await Context.SaveChangesAsync();

        // Act
        var result = await Context.Accounts
            .Include(a => a.Transactions)
                .ThenInclude(t => t.Category)
            .Where(a => a.Id == account.Id)
            .Select(a => new
            {
                Account = a,
                TransactionCount = a.Transactions.Count,
                TotalExpenses = a.Transactions
                    .Where(t => t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount),
                Categories = a.Transactions
                    .Select(t => t.Category!.Name)
                    .Distinct()
                    .ToList()
            })
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account.Id, result.Account.Id);
        Assert.Equal(5, result.TransactionCount);
        Assert.True(result.TotalExpenses > 0);
        Assert.NotEmpty(result.Categories);
    }

    public override void Dispose()
    {
        CleanupDatabaseAsync().Wait();
        base.Dispose();
    }
}