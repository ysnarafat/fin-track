using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Infrastructure.Data;
using FinTrack.Infrastructure.Repositories;
using FinTrack.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinTrack.Tests.Unit.Infrastructure.Repositories;

/// <summary>
/// Unit tests for CategoryRepository
/// </summary>
public class CategoryRepositoryTests : IDisposable
{
    private readonly FinTrackDbContext _context;
    private readonly Mock<ILogger<CategoryRepository>> _mockLogger;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FinTrackDbContext(options);
        _mockLogger = new Mock<ILogger<CategoryRepository>>();
        _repository = new CategoryRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByTypeAsync_WithValidType_ShouldReturnCategoriesOfSpecificType()
    {
        // Arrange
        var expenseCategory = TestDataBuilder.CreateCategory("Food", TransactionType.Expense);
        var incomeCategory = TestDataBuilder.CreateCategory("Salary", TransactionType.Income);

        _context.Categories.AddRange(expenseCategory, incomeCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTypeAsync(TransactionType.Expense);

        // Assert
        Assert.Single(result);
        Assert.Equal(TransactionType.Expense, result.First().CategoryType);
        Assert.Equal("Food", result.First().Name);
    }

    [Fact]
    public async Task GetActiveAsync_ShouldReturnOnlyActiveCategories()
    {
        // Arrange
        var activeCategory = TestDataBuilder.CreateCategory("Active Category", isActive: true);
        var inactiveCategory = TestDataBuilder.CreateCategory("Inactive Category", isActive: false);

        _context.Categories.AddRange(activeCategory, inactiveCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result.First().IsActive);
        Assert.Equal("Active Category", result.First().Name);
    }

    [Fact]
    public async Task GetRootCategoriesAsync_ShouldReturnOnlyCategoriesWithoutParent()
    {
        // Arrange
        var rootCategory = TestDataBuilder.CreateCategory("Root Category");
        var childCategory = TestDataBuilder.CreateCategory("Child Category");
        childCategory.ParentCategoryId = rootCategory.Id;

        _context.Categories.AddRange(rootCategory, childCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRootCategoriesAsync();

        // Assert
        Assert.Single(result);
        Assert.Null(result.First().ParentCategoryId);
        Assert.Equal("Root Category", result.First().Name);
    }

    [Fact]
    public async Task GetSubCategoriesAsync_ShouldReturnChildCategories()
    {
        // Arrange
        var parentCategory = TestDataBuilder.CreateCategory("Parent Category");
        _context.Categories.Add(parentCategory);
        await _context.SaveChangesAsync();

        var childCategory1 = TestDataBuilder.CreateCategory("Child 1");
        childCategory1.ParentCategoryId = parentCategory.Id;
        var childCategory2 = TestDataBuilder.CreateCategory("Child 2");
        childCategory2.ParentCategoryId = parentCategory.Id;
        var otherCategory = TestDataBuilder.CreateCategory("Other Category");

        _context.Categories.AddRange(childCategory1, childCategory2, otherCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSubCategoriesAsync(parentCategory.Id);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Equal(parentCategory.Id, c.ParentCategoryId));
    }

    [Fact]
    public async Task GetHierarchyAsync_ShouldReturnCorrectHierarchy()
    {
        // Arrange
        var rootCategory = TestDataBuilder.CreateCategory("Root");
        _context.Categories.Add(rootCategory);
        await _context.SaveChangesAsync();

        var childCategory = TestDataBuilder.CreateCategory("Child");
        childCategory.ParentCategoryId = rootCategory.Id;
        _context.Categories.Add(childCategory);
        await _context.SaveChangesAsync();

        var grandChildCategory = TestDataBuilder.CreateCategory("GrandChild");
        grandChildCategory.ParentCategoryId = childCategory.Id;
        _context.Categories.Add(grandChildCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetHierarchyAsync();

        // Assert
        Assert.Single(result); // Only one root category
        var rootHierarchy = result.First();
        Assert.Equal("Root", rootHierarchy.Category.Name);
        Assert.Equal(0, rootHierarchy.Level);
        Assert.Single(rootHierarchy.SubCategories);
        
        var childHierarchy = rootHierarchy.SubCategories.First();
        Assert.Equal("Child", childHierarchy.Category.Name);
        Assert.Equal(1, childHierarchy.Level);
        Assert.Single(childHierarchy.SubCategories);
        
        var grandChildHierarchy = childHierarchy.SubCategories.First();
        Assert.Equal("GrandChild", grandChildHierarchy.Category.Name);
        Assert.Equal(2, grandChildHierarchy.Level);
    }

    [Fact]
    public async Task GetWithTransactionCountsAsync_ShouldReturnCategoriesWithCounts()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Test Category");
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var account = TestDataBuilder.CreateAccount("Test Account");
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;

        // Add transactions
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(100m, "Transaction 1", categoryId: category.Id, accountId: account.Id, date: startDate.AddDays(5)),
            TestDataBuilder.CreateTransaction(200m, "Transaction 2", categoryId: category.Id, accountId: account.Id, date: startDate.AddDays(10)),
            TestDataBuilder.CreateTransaction(300m, "Old Transaction", categoryId: category.Id, accountId: account.Id, date: startDate.AddDays(-10)) // Outside range
        };

        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithTransactionCountsAsync(startDate, endDate);

        // Assert
        var categoryWithStats = result.First();
        Assert.Equal(2, categoryWithStats.TransactionCount); // Only transactions in date range
        Assert.NotNull(categoryWithStats.FirstTransactionDate);
        Assert.NotNull(categoryWithStats.LastTransactionDate);
    }

    [Fact]
    public async Task GetSpendingTotalsAsync_ShouldReturnCorrectSpendingData()
    {
        // Arrange
        var category1 = TestDataBuilder.CreateCategory("Category 1", TransactionType.Expense);
        var category2 = TestDataBuilder.CreateCategory("Category 2", TransactionType.Expense);
        _context.Categories.AddRange(category1, category2);
        await _context.SaveChangesAsync();

        var account = TestDataBuilder.CreateAccount("Test Account");
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;

        // Add expense transactions
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(100m, "Expense 1", TransactionType.Expense, categoryId: category1.Id, accountId: account.Id, date: startDate.AddDays(5)),
            TestDataBuilder.CreateTransaction(200m, "Expense 2", TransactionType.Expense, categoryId: category1.Id, accountId: account.Id, date: startDate.AddDays(10)),
            TestDataBuilder.CreateTransaction(150m, "Expense 3", TransactionType.Expense, categoryId: category2.Id, accountId: account.Id, date: startDate.AddDays(15)),
            TestDataBuilder.CreateTransaction(500m, "Income", TransactionType.Income, categoryId: category1.Id, accountId: account.Id, date: startDate.AddDays(20)) // Should be ignored
        };

        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSpendingTotalsAsync(startDate, endDate);

        // Assert
        Assert.Equal(2, result.Count());
        
        var category1Spending = result.First(c => c.Category.Name == "Category 1");
        Assert.Equal(300m, category1Spending.TotalSpending); // 100 + 200
        Assert.Equal(2, category1Spending.TransactionCount);
        Assert.Equal(150m, category1Spending.AverageTransactionAmount);
        
        var category2Spending = result.First(c => c.Category.Name == "Category 2");
        Assert.Equal(150m, category2Spending.TotalSpending);
        Assert.Equal(1, category2Spending.TransactionCount);
    }

    [Fact]
    public async Task GetSystemCategoriesAsync_ShouldReturnOnlySystemCategories()
    {
        // Arrange
        var systemCategory = TestDataBuilder.CreateCategory("System Category", isSystem: true);
        var userCategory = TestDataBuilder.CreateCategory("User Category", isSystem: false);

        _context.Categories.AddRange(systemCategory, userCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSystemCategoriesAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result.First().IsSystem);
        Assert.Equal("System Category", result.First().Name);
    }

    [Fact]
    public async Task GetUserCategoriesAsync_ShouldReturnOnlyUserCategories()
    {
        // Arrange
        var systemCategory = TestDataBuilder.CreateCategory("System Category", isSystem: true);
        var userCategory = TestDataBuilder.CreateCategory("User Category", isSystem: false);

        _context.Categories.AddRange(systemCategory, userCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserCategoriesAsync();

        // Assert
        Assert.Single(result);
        Assert.False(result.First().IsSystem);
        Assert.Equal("User Category", result.First().Name);
    }

    [Fact]
    public async Task SearchByNameAsync_WithMatchingTerm_ShouldReturnMatchingCategories()
    {
        // Arrange
        var category1 = TestDataBuilder.CreateCategory("Food & Dining");
        var category2 = TestDataBuilder.CreateCategory("Food Shopping");
        var category3 = TestDataBuilder.CreateCategory("Transportation");

        _context.Categories.AddRange(category1, category2, category3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByNameAsync("Food");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Contains("Food", c.Name));
    }

    [Fact]
    public async Task SearchByNameAsync_WithEmptyTerm_ShouldReturnEmpty()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Test Category");
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByNameAsync("");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUsedInDateRangeAsync_ShouldReturnCategoriesWithTransactionsInRange()
    {
        // Arrange
        var usedCategory = TestDataBuilder.CreateCategory("Used Category");
        var unusedCategory = TestDataBuilder.CreateCategory("Unused Category");
        _context.Categories.AddRange(usedCategory, unusedCategory);
        await _context.SaveChangesAsync();

        var account = TestDataBuilder.CreateAccount("Test Account");
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;

        var transaction = TestDataBuilder.CreateTransaction(100m, "Test Transaction", 
            categoryId: usedCategory.Id, accountId: account.Id, date: startDate.AddDays(5));
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUsedInDateRangeAsync(startDate, endDate);

        // Assert
        Assert.Single(result);
        Assert.Equal("Used Category", result.First().Name);
    }

    [Fact]
    public async Task UpdateSortOrdersAsync_ShouldUpdateMultipleCategorySortOrders()
    {
        // Arrange
        var category1 = TestDataBuilder.CreateCategory("Category 1");
        category1.SortOrder = 1;
        var category2 = TestDataBuilder.CreateCategory("Category 2");
        category2.SortOrder = 2;
        
        _context.Categories.AddRange(category1, category2);
        await _context.SaveChangesAsync();

        var sortOrders = new Dictionary<int, int>
        {
            { category1.Id, 10 },
            { category2.Id, 5 }
        };

        // Act
        var updatedCount = await _repository.UpdateSortOrdersAsync(sortOrders);

        // Assert
        Assert.Equal(2, updatedCount);
        
        var updatedCategory1 = await _context.Categories.FindAsync(category1.Id);
        var updatedCategory2 = await _context.Categories.FindAsync(category2.Id);
        
        Assert.Equal(10, updatedCategory1!.SortOrder);
        Assert.Equal(5, updatedCategory2!.SortOrder);
    }

    [Fact]
    public async Task CanDeleteAsync_WithTransactions_ShouldReturnFalse()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Test Category");
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var account = TestDataBuilder.CreateAccount("Test Account");
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var transaction = TestDataBuilder.CreateTransaction(100m, "Test Transaction", 
            categoryId: category.Id, accountId: account.Id);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var canDelete = await _repository.CanDeleteAsync(category.Id);

        // Assert
        Assert.False(canDelete);
    }

    [Fact]
    public async Task CanDeleteAsync_WithSubCategories_ShouldReturnFalse()
    {
        // Arrange
        var parentCategory = TestDataBuilder.CreateCategory("Parent Category");
        _context.Categories.Add(parentCategory);
        await _context.SaveChangesAsync();

        var childCategory = TestDataBuilder.CreateCategory("Child Category");
        childCategory.ParentCategoryId = parentCategory.Id;
        _context.Categories.Add(childCategory);
        await _context.SaveChangesAsync();

        // Act
        var canDelete = await _repository.CanDeleteAsync(parentCategory.Id);

        // Assert
        Assert.False(canDelete);
    }

    [Fact]
    public async Task CanDeleteAsync_WithBudgets_ShouldReturnFalse()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Test Category");
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var budget = TestDataBuilder.CreateBudget(500m, category.Id);
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        // Act
        var canDelete = await _repository.CanDeleteAsync(category.Id);

        // Assert
        Assert.False(canDelete);
    }

    [Fact]
    public async Task CanDeleteAsync_WithNoConstraints_ShouldReturnTrue()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Test Category");
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var canDelete = await _repository.CanDeleteAsync(category.Id);

        // Assert
        Assert.True(canDelete);
    }

    [Fact]
    public async Task GetCategoryPathAsync_ShouldReturnCorrectPath()
    {
        // Arrange
        var grandParent = TestDataBuilder.CreateCategory("GrandParent");
        _context.Categories.Add(grandParent);
        await _context.SaveChangesAsync();

        var parent = TestDataBuilder.CreateCategory("Parent");
        parent.ParentCategoryId = grandParent.Id;
        _context.Categories.Add(parent);
        await _context.SaveChangesAsync();

        var child = TestDataBuilder.CreateCategory("Child");
        child.ParentCategoryId = parent.Id;
        _context.Categories.Add(child);
        await _context.SaveChangesAsync();

        // Act
        var path = await _repository.GetCategoryPathAsync(child.Id);

        // Assert
        var pathList = path.ToList();
        Assert.Equal(3, pathList.Count);
        Assert.Equal("GrandParent", pathList[0].Name);
        Assert.Equal("Parent", pathList[1].Name);
        Assert.Equal("Child", pathList[2].Name);
    }

    [Fact]
    public async Task GetOrderedAsync_ShouldReturnCategoriesOrderedBySortOrder()
    {
        // Arrange
        var category1 = TestDataBuilder.CreateCategory("Category C");
        category1.SortOrder = 3;
        var category2 = TestDataBuilder.CreateCategory("Category A");
        category2.SortOrder = 1;
        var category3 = TestDataBuilder.CreateCategory("Category B");
        category3.SortOrder = 2;

        _context.Categories.AddRange(category1, category2, category3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrderedAsync();

        // Assert
        var orderedList = result.ToList();
        Assert.Equal("Category A", orderedList[0].Name);
        Assert.Equal("Category B", orderedList[1].Name);
        Assert.Equal("Category C", orderedList[2].Name);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}