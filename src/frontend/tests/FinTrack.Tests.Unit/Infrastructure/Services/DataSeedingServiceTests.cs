using FinTrack.Core.Enums;
using FinTrack.Infrastructure.Data;
using FinTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FinTrack.Tests.Unit.Infrastructure.Services;

public class DataSeedingServiceTests : IDisposable
{
    private readonly FinTrackDbContext _context;
    private readonly DataSeedingService _service;

    public DataSeedingServiceTests()
    {
        var options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FinTrackDbContext(options);
        
        var logger = new LoggerFactory().CreateLogger<DataSeedingService>();
        _service = new DataSeedingService(_context, logger);
    }

    [Fact]
    public async Task SeedDefaultCategoriesAsync_ShouldCreateExpenseAndIncomeCategories()
    {
        // Act
        await _service.SeedDefaultCategoriesAsync();
        await _context.SaveChangesAsync(); // Save the changes to make them available for queries

        // Assert
        var expenseCategories = await _context.Categories
            .Where(c => c.CategoryType == TransactionType.Expense)
            .CountAsync();
        
        var incomeCategories = await _context.Categories
            .Where(c => c.CategoryType == TransactionType.Income)
            .CountAsync();

        Assert.True(expenseCategories > 0, "Should create expense categories");
        Assert.True(incomeCategories > 0, "Should create income categories");
        
        // Verify specific categories exist
        Assert.True(await _context.Categories.AnyAsync(c => c.Name == "Food & Dining"));
        Assert.True(await _context.Categories.AnyAsync(c => c.Name == "Salary"));
    }

    [Fact]
    public async Task SeedDefaultAccountTypesAsync_ShouldCreateDefaultAccounts()
    {
        // Act
        await _service.SeedDefaultAccountTypesAsync();
        await _context.SaveChangesAsync(); // Save the changes to make them available for queries

        // Assert
        var accounts = await _context.Accounts.CountAsync();
        Assert.True(accounts > 0, "Should create default accounts");
        
        var checkingAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Type == AccountType.Checking);
        
        Assert.NotNull(checkingAccount);
        Assert.Equal("Primary Checking", checkingAccount.Name);
    }

    [Fact]
    public async Task SeedAllDataAsync_WithSampleData_ShouldCreateAllData()
    {
        // Act
        await _service.SeedAllDataAsync(true);

        // Assert
        var categories = await _context.Categories.CountAsync();
        var accounts = await _context.Accounts.CountAsync();
        var transactions = await _context.Transactions.CountAsync();
        var budgets = await _context.Budgets.CountAsync();
        var goals = await _context.Goals.CountAsync();

        Assert.True(categories > 0, "Should create categories");
        Assert.True(accounts > 0, "Should create accounts");
        Assert.True(transactions > 0, "Should create sample transactions");
        Assert.True(budgets > 0, "Should create sample budgets");
        Assert.True(goals > 0, "Should create sample goals");
    }

    [Fact]
    public async Task IsDataSeededAsync_ShouldReturnTrueWhenDataExists()
    {
        // Arrange
        await _service.SeedAllDataAsync(false);

        // Act
        var isSeeded = await _service.IsDataSeededAsync();

        // Assert
        Assert.True(isSeeded);
    }

    [Fact]
    public async Task IsDataSeededAsync_ShouldReturnFalseWhenNoDataExists()
    {
        // Act
        var isSeeded = await _service.IsDataSeededAsync();

        // Assert
        Assert.False(isSeeded);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}