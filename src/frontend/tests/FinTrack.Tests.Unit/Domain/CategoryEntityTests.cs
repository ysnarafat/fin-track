using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Tests.Unit.Helpers;

namespace FinTrack.Tests.Unit.Domain;

/// <summary>
/// Comprehensive unit tests for Category entity
/// </summary>
public class CategoryEntityTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var category = new Category();

        // Assert
        Assert.True(category.IsActive);
        Assert.False(category.IsSystem);
        Assert.Equal(0, category.SortOrder);
        Assert.Equal("#6B7280", category.Color);
        Assert.Empty(category.Transactions);
        Assert.Empty(category.SubCategories);
        Assert.Equal(string.Empty, category.Name);
    }

    [Fact]
    public void FullPath_WithNoParent_ShouldReturnName()
    {
        // Arrange
        var category = TestDataBuilder.Category()
            .WithName("Food")
            .Build();

        // Act
        var fullPath = category.FullPath;

        // Assert
        Assert.Equal("Food", fullPath);
    }

    [Fact]
    public void FullPath_WithParent_ShouldReturnHierarchicalPath()
    {
        // Arrange
        var parentCategory = TestDataBuilder.Category()
            .WithId(1)
            .WithName("Expenses")
            .Build();

        var childCategory = TestDataBuilder.Category()
            .WithId(2)
            .WithName("Food")
            .WithParent(1)
            .Build();

        childCategory.ParentCategory = parentCategory;

        // Act
        var fullPath = childCategory.FullPath;

        // Assert
        Assert.Equal("Expenses > Food", fullPath);
    }

    [Fact]
    public void FullPath_WithMultipleLevels_ShouldReturnCompleteHierarchy()
    {
        // Arrange
        var grandParent = TestDataBuilder.Category()
            .WithName("All Expenses")
            .Build();

        var parent = TestDataBuilder.Category()
            .WithName("Food & Dining")
            .Build();
        parent.ParentCategory = grandParent;

        var child = TestDataBuilder.Category()
            .WithName("Restaurants")
            .Build();
        child.ParentCategory = parent;

        // Act
        var fullPath = child.FullPath;

        // Assert
        Assert.Equal("All Expenses > Food & Dining > Restaurants", fullPath);
    }

    [Theory]
    [InlineData(0, 0)] // Root level
    [InlineData(1, 1)] // One level deep
    [InlineData(2, 2)] // Two levels deep
    public void Level_ShouldReturnCorrectDepth(int expectedLevel, int actualParentLevels)
    {
        // Arrange
        var category = new Category { Name = "Test" };
        var current = category;

        // Create parent hierarchy
        for (int i = 0; i < actualParentLevels; i++)
        {
            var parent = new Category { Name = $"Parent{i}" };
            current.ParentCategory = parent;
            current = parent;
        }

        // Act
        var level = category.Level;

        // Assert
        Assert.Equal(expectedLevel, level);
    }

    [Fact]
    public void HasSubCategories_WithNoSubCategories_ShouldReturnFalse()
    {
        // Arrange
        var category = TestDataBuilder.Category().Build();

        // Act & Assert
        Assert.False(category.HasSubCategories);
    }

    [Fact]
    public void HasSubCategories_WithActiveSubCategories_ShouldReturnTrue()
    {
        // Arrange
        var category = TestDataBuilder.Category().Build();
        var subCategory = TestDataBuilder.Category()
            .WithName("Subcategory")
            .Build();

        category.SubCategories.Add(subCategory);

        // Act & Assert
        Assert.True(category.HasSubCategories);
    }

    [Fact]
    public void HasSubCategories_WithOnlyDeletedSubCategories_ShouldReturnFalse()
    {
        // Arrange
        var category = TestDataBuilder.Category().Build();
        var deletedSubCategory = TestDataBuilder.Category()
            .WithName("Deleted Subcategory")
            .AsDeleted()
            .Build();

        category.SubCategories.Add(deletedSubCategory);

        // Act & Assert
        Assert.False(category.HasSubCategories);
    }

    [Fact]
    public void ActiveSubCategories_ShouldReturnOnlyActiveNonDeletedCategories()
    {
        // Arrange
        var category = TestDataBuilder.Category().Build();
        
        var activeCategory = TestDataBuilder.Category()
            .WithName("Active")
            .Build();
        
        var inactiveCategory = TestDataBuilder.Category()
            .WithName("Inactive")
            .AsInactive()
            .Build();
        
        var deletedCategory = TestDataBuilder.Category()
            .WithName("Deleted")
            .AsDeleted()
            .Build();

        category.SubCategories.Add(activeCategory);
        category.SubCategories.Add(inactiveCategory);
        category.SubCategories.Add(deletedCategory);

        // Act
        var activeSubCategories = category.ActiveSubCategories.ToList();

        // Assert
        Assert.Single(activeSubCategories);
        Assert.Equal("Active", activeSubCategories[0].Name);
    }

    [Fact]
    public void ActiveTransactions_ShouldReturnOnlyNonDeletedTransactions()
    {
        // Arrange
        var category = TestDataBuilder.Category().Build();
        
        var activeTransaction = TestDataBuilder.Transaction()
            .WithDescription("Active Transaction")
            .Build();
        
        var deletedTransaction = TestDataBuilder.Transaction()
            .WithDescription("Deleted Transaction")
            .AsDeleted()
            .Build();

        category.Transactions.Add(activeTransaction);
        category.Transactions.Add(deletedTransaction);

        // Act
        var activeTransactions = category.ActiveTransactions.ToList();

        // Assert
        Assert.Single(activeTransactions);
        Assert.Equal("Active Transaction", activeTransactions[0].Description);
    }

    [Theory]
    [InlineData("", false)] // Empty name
    [InlineData("   ", false)] // Whitespace name
    [InlineData(null!, false)] // Null name
    [InlineData("Valid Name", true)] // Valid name
    public void IsValid_WithDifferentNames_ShouldReturnExpectedResult(string? name, bool expectedValid)
    {
        // Arrange
        var category = TestDataBuilder.Category()
            .WithName(name)
            .Build();

        // Act
        var isValid = category.IsValid();

        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public void IsValid_WithSelfAsParent_ShouldReturnFalse()
    {
        // Arrange
        var category = TestDataBuilder.Category()
            .WithId(1)
            .WithName("Test Category")
            .WithParent(1) // Self as parent
            .Build();

        // Act
        var isValid = category.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("#FF0000", true)] // Valid hex color
    [InlineData("#123ABC", true)] // Valid hex color with letters
    [InlineData("#000", false)] // Too short
    [InlineData("FF0000", false)] // Missing #
    [InlineData("#GGGGGG", false)] // Invalid hex characters
    [InlineData("", true)] // Empty is valid (uses default)
    [InlineData(null!, true)] // Null is valid (uses default)
    public void IsValid_WithDifferentColors_ShouldReturnExpectedResult(string? color, bool expectedValid)
    {
        // Arrange
        var category = TestDataBuilder.Category()
            .WithName("Test Category")
            .WithColor(color)
            .Build();

        // Act
        var isValid = category.IsValid();

        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    [Theory]
    [InlineData(-100.0, false)] // Negative budget limit
    [InlineData(0.0, true)] // Zero budget limit
    [InlineData(100.0, true)] // Positive budget limit
    [InlineData(null, true)] // No budget limit
    public void IsValid_WithDifferentBudgetLimits_ShouldReturnExpectedResult(double? budgetLimit, bool expectedValid)
    {
        // Arrange
        var category = TestDataBuilder.Category()
            .WithName("Test Category")
            .Build();
            
        if (budgetLimit.HasValue)
            category.BudgetLimit = (decimal)budgetLimit.Value;

        // Act
        var isValid = category.IsValid();

        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public void CalculateSpending_WithExpenseTransactions_ShouldReturnCorrectTotal()
    {
        // Arrange
        var category = TestDataBuilder.Category().Build();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        var expense1 = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithType(TransactionType.Expense)
            .WithDate(new DateTime(2024, 1, 15))
            .Build();

        var expense2 = TestDataBuilder.Transaction()
            .WithAmount(50m)
            .WithType(TransactionType.Expense)
            .WithDate(new DateTime(2024, 1, 20))
            .Build();

        var income = TestDataBuilder.Transaction()
            .WithAmount(200m)
            .WithType(TransactionType.Income)
            .WithDate(new DateTime(2024, 1, 10))
            .Build();

        category.Transactions.Add(expense1);
        category.Transactions.Add(expense2);
        category.Transactions.Add(income);

        // Act
        var totalSpending = category.CalculateSpending(startDate, endDate);

        // Assert
        Assert.Equal(150m, totalSpending); // Only expenses should be counted
    }

    [Fact]
    public void CalculateSpending_WithTransactionsOutsideDateRange_ShouldExcludeThem()
    {
        // Arrange
        var category = TestDataBuilder.Category().Build();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        var expenseInRange = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithType(TransactionType.Expense)
            .WithDate(new DateTime(2024, 1, 15))
            .Build();

        var expenseOutOfRange = TestDataBuilder.Transaction()
            .WithAmount(50m)
            .WithType(TransactionType.Expense)
            .WithDate(new DateTime(2024, 2, 1))
            .Build();

        category.Transactions.Add(expenseInRange);
        category.Transactions.Add(expenseOutOfRange);

        // Act
        var totalSpending = category.CalculateSpending(startDate, endDate);

        // Assert
        Assert.Equal(100m, totalSpending);
    }

    [Fact]
    public void CalculateSpendingWithSubCategories_ShouldIncludeSubCategorySpending()
    {
        // Arrange
        var parentCategory = TestDataBuilder.Category()
            .WithName("Food")
            .Build();

        var subCategory = TestDataBuilder.Category()
            .WithName("Restaurants")
            .Build();

        parentCategory.SubCategories.Add(subCategory);

        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        var parentExpense = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithType(TransactionType.Expense)
            .WithDate(new DateTime(2024, 1, 15))
            .Build();

        var subCategoryExpense = TestDataBuilder.Transaction()
            .WithAmount(75m)
            .WithType(TransactionType.Expense)
            .WithDate(new DateTime(2024, 1, 20))
            .Build();

        parentCategory.Transactions.Add(parentExpense);
        subCategory.Transactions.Add(subCategoryExpense);

        // Act
        var totalSpending = parentCategory.CalculateSpendingWithSubCategories(startDate, endDate);

        // Assert
        Assert.Equal(175m, totalSpending); // Parent + subcategory spending
    }

    [Fact]
    public void BudgetUtilizationPercentage_WithNoBudgetLimit_ShouldReturnNull()
    {
        // Arrange
        var category = TestDataBuilder.Category()
            .Build(); // No budget limit set

        // Act
        var utilization = category.BudgetUtilizationPercentage;

        // Assert
        Assert.Null(utilization);
    }

    [Fact]
    public void BudgetUtilizationPercentage_WithZeroBudgetLimit_ShouldReturnNull()
    {
        // Arrange
        var category = TestDataBuilder.Category()
            .WithBudgetLimit(0m)
            .Build();

        // Act
        var utilization = category.BudgetUtilizationPercentage;

        // Assert
        Assert.Null(utilization);
    }

    [Fact]
    public void BudgetUtilizationPercentage_WithCurrentMonthSpending_ShouldReturnCorrectPercentage()
    {
        // Arrange
        var category = TestDataBuilder.Category()
            .WithBudgetLimit(500m)
            .Build();

        var currentMonth = DateTime.Today;
        var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);

        var expense = TestDataBuilder.Transaction()
            .WithAmount(150m)
            .WithType(TransactionType.Expense)
            .WithDate(startOfMonth.AddDays(10))
            .Build();

        category.Transactions.Add(expense);

        // Act
        var utilization = category.BudgetUtilizationPercentage;

        // Assert
        Assert.Equal(30m, utilization); // 150 / 500 * 100 = 30%
    }

    [Fact]
    public void BudgetUtilizationPercentage_WithSpendingExceedingBudget_ShouldReturnOverHundredPercent()
    {
        // Arrange
        var category = TestDataBuilder.Category()
            .WithBudgetLimit(100m)
            .Build();

        var currentMonth = DateTime.Today;
        var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);

        var expense = TestDataBuilder.Transaction()
            .WithAmount(150m)
            .WithType(TransactionType.Expense)
            .WithDate(startOfMonth.AddDays(10))
            .Build();

        category.Transactions.Add(expense);

        // Act
        var utilization = category.BudgetUtilizationPercentage;

        // Assert
        Assert.Equal(150m, utilization); // 150 / 100 * 100 = 150%
    }

    [Fact]
    public void BudgetUtilizationPercentage_WithPreviousMonthSpending_ShouldNotIncludeIt()
    {
        // Arrange
        var category = TestDataBuilder.Category()
            .WithBudgetLimit(500m)
            .Build();

        var previousMonth = DateTime.Today.AddMonths(-1);

        var currentMonthExpense = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithType(TransactionType.Expense)
            .WithDate(DateTime.Today)
            .Build();

        var previousMonthExpense = TestDataBuilder.Transaction()
            .WithAmount(200m)
            .WithType(TransactionType.Expense)
            .WithDate(previousMonth)
            .Build();

        category.Transactions.Add(currentMonthExpense);
        category.Transactions.Add(previousMonthExpense);

        // Act
        var utilization = category.BudgetUtilizationPercentage;

        // Assert
        Assert.Equal(20m, utilization); // Only current month: 100 / 500 * 100 = 20%
    }
}