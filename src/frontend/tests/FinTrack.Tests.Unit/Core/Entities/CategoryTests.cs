using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Tests.Unit.Core.Entities;

/// <summary>
/// Unit tests for Category entity
/// </summary>
public class CategoryTests
{
    [Fact]
    public void Constructor_ShouldInitializeDefaultValues()
    {
        // Arrange & Act
        var category = new Category();

        // Assert
        Assert.True(category.IsActive);
        Assert.False(category.IsSystem);
        Assert.Equal(0, category.SortOrder);
        Assert.Equal("#6B7280", category.Color);
        Assert.Equal(TransactionType.Expense, category.CategoryType);
        Assert.Empty(category.Name);
        Assert.Null(category.Description);
        Assert.Null(category.Icon);
        Assert.Null(category.ParentCategoryId);
        Assert.Null(category.BudgetLimit);
        Assert.NotNull(category.Transactions);
        Assert.NotNull(category.SubCategories);
        Assert.NotNull(category.Budgets);
    }

    [Fact]
    public void FullPath_WithNoParent_ShouldReturnName()
    {
        // Arrange
        var category = new Category { Name = "Food" };

        // Act
        var result = category.FullPath;

        // Assert
        Assert.Equal("Food", result);
    }

    [Fact]
    public void FullPath_WithParent_ShouldReturnFullPath()
    {
        // Arrange
        var parentCategory = new Category { Name = "Food" };
        var childCategory = new Category 
        { 
            Name = "Restaurants", 
            ParentCategory = parentCategory 
        };

        // Act
        var result = childCategory.FullPath;

        // Assert
        Assert.Equal("Food > Restaurants", result);
    }

    [Fact]
    public void FullPath_WithMultipleLevels_ShouldReturnFullPath()
    {
        // Arrange
        var grandParent = new Category { Name = "Expenses" };
        var parent = new Category 
        { 
            Name = "Food", 
            ParentCategory = grandParent 
        };
        var child = new Category 
        { 
            Name = "Restaurants", 
            ParentCategory = parent 
        };

        // Act
        var result = child.FullPath;

        // Assert
        Assert.Equal("Expenses > Food > Restaurants", result);
    }

    [Fact]
    public void Level_WithNoParent_ShouldReturnZero()
    {
        // Arrange
        var category = new Category { Name = "Food" };

        // Act
        var result = category.Level;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Level_WithParent_ShouldReturnCorrectLevel()
    {
        // Arrange
        var parentCategory = new Category { Name = "Food" };
        var childCategory = new Category 
        { 
            Name = "Restaurants", 
            ParentCategory = parentCategory 
        };

        // Act
        var result = childCategory.Level;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Level_WithMultipleLevels_ShouldReturnCorrectLevel()
    {
        // Arrange
        var grandParent = new Category { Name = "Expenses" };
        var parent = new Category 
        { 
            Name = "Food", 
            ParentCategory = grandParent 
        };
        var child = new Category 
        { 
            Name = "Restaurants", 
            ParentCategory = parent 
        };

        // Act
        var result = child.Level;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void HasSubCategories_WithNoSubCategories_ShouldReturnFalse()
    {
        // Arrange
        var category = new Category();

        // Act
        var result = category.HasSubCategories;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasSubCategories_WithActiveSubCategories_ShouldReturnTrue()
    {
        // Arrange
        var category = new Category();
        category.SubCategories.Add(new Category { IsActive = true, IsDeleted = false });

        // Act
        var result = category.HasSubCategories;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasSubCategories_WithDeletedSubCategories_ShouldReturnFalse()
    {
        // Arrange
        var category = new Category();
        category.SubCategories.Add(new Category { IsActive = true, IsDeleted = true });

        // Act
        var result = category.HasSubCategories;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ActiveSubCategories_ShouldReturnOnlyActiveNonDeletedCategories()
    {
        // Arrange
        var category = new Category();
        category.SubCategories.Add(new Category { Name = "Active1", IsActive = true, IsDeleted = false });
        category.SubCategories.Add(new Category { Name = "Inactive", IsActive = false, IsDeleted = false });
        category.SubCategories.Add(new Category { Name = "Deleted", IsActive = true, IsDeleted = true });
        category.SubCategories.Add(new Category { Name = "Active2", IsActive = true, IsDeleted = false });

        // Act
        var result = category.ActiveSubCategories.ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "Active1");
        Assert.Contains(result, c => c.Name == "Active2");
    }

    [Fact]
    public void ActiveTransactions_ShouldReturnOnlyNonDeletedTransactions()
    {
        // Arrange
        var category = new Category();
        category.Transactions.Add(new Transaction { Description = "Active1", IsDeleted = false });
        category.Transactions.Add(new Transaction { Description = "Deleted", IsDeleted = true });
        category.Transactions.Add(new Transaction { Description = "Active2", IsDeleted = false });

        // Act
        var result = category.ActiveTransactions.ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Description == "Active1");
        Assert.Contains(result, t => t.Description == "Active2");
    }

    [Fact]
    public void IsValid_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var category = new Category
        {
            Name = "Food",
            Color = "#FF0000",
            BudgetLimit = 500
        };

        // Act
        var result = category.IsValid();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsValid_WithInvalidName_ShouldReturnFalse(string name)
    {
        // Arrange
        var category = new Category { Name = name };

        // Act
        var result = category.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithSelfAsParent_ShouldReturnFalse()
    {
        // Arrange
        var category = new Category 
        { 
            Id = 1,
            Name = "Food",
            ParentCategoryId = 1 // Same as Id
        };

        // Act
        var result = category.IsValid();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("FF0000")] // Missing #
    [InlineData("#FF00")] // Too short
    [InlineData("#FF00000")] // Too long
    [InlineData("#GGGGGG")] // Invalid hex characters
    public void IsValid_WithInvalidColor_ShouldReturnFalse(string color)
    {
        // Arrange
        var category = new Category 
        { 
            Name = "Food",
            Color = color
        };

        // Act
        var result = category.IsValid();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("#FF0000")]
    [InlineData("#00FF00")]
    [InlineData("#0000FF")]
    [InlineData("#FFFFFF")]
    [InlineData("#000000")]
    [InlineData("#123ABC")]
    [InlineData("#abc123")]
    public void IsValid_WithValidColor_ShouldReturnTrue(string color)
    {
        // Arrange
        var category = new Category 
        { 
            Name = "Food",
            Color = color
        };

        // Act
        var result = category.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithNegativeBudgetLimit_ShouldReturnFalse()
    {
        // Arrange
        var category = new Category 
        { 
            Name = "Food",
            BudgetLimit = -100
        };

        // Act
        var result = category.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CalculateSpending_ShouldSumExpenseTransactionsInDateRange()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        
        var category = new Category();
        category.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 100, 
            Date = new DateTime(2024, 1, 15),
            IsDeleted = false
        });
        category.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 50, 
            Date = new DateTime(2024, 1, 25),
            IsDeleted = false
        });
        category.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Income, 
            Amount = 200, 
            Date = new DateTime(2024, 1, 20),
            IsDeleted = false
        });
        category.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 75, 
            Date = new DateTime(2024, 2, 5), // Outside date range
            IsDeleted = false
        });
        category.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 25, 
            Date = new DateTime(2024, 1, 10),
            IsDeleted = true // Deleted transaction
        });

        // Act
        var result = category.CalculateSpending(startDate, endDate);

        // Assert
        Assert.Equal(150, result); // 100 + 50
    }

    [Fact]
    public void CalculateSpendingWithSubCategories_ShouldIncludeSubCategorySpending()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        
        var parentCategory = new Category();
        parentCategory.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 100, 
            Date = new DateTime(2024, 1, 15),
            IsDeleted = false
        });

        var subCategory = new Category { IsActive = true, IsDeleted = false };
        subCategory.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 50, 
            Date = new DateTime(2024, 1, 20),
            IsDeleted = false
        });

        parentCategory.SubCategories.Add(subCategory);

        // Act
        var result = parentCategory.CalculateSpendingWithSubCategories(startDate, endDate);

        // Assert
        Assert.Equal(150, result); // 100 (parent) + 50 (sub)
    }

    [Fact]
    public void BudgetUtilizationPercentage_WithNoBudgetLimit_ShouldReturnNull()
    {
        // Arrange
        var category = new Category { BudgetLimit = null };

        // Act
        var result = category.BudgetUtilizationPercentage;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void BudgetUtilizationPercentage_WithZeroBudgetLimit_ShouldReturnNull()
    {
        // Arrange
        var category = new Category { BudgetLimit = 0 };

        // Act
        var result = category.BudgetUtilizationPercentage;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void BudgetUtilizationPercentage_WithSpendingAndBudget_ShouldCalculateCorrectly()
    {
        // Arrange
        var category = new Category { BudgetLimit = 200 };
        
        // Add transaction for current month
        var currentMonth = DateTime.Today;
        var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        
        category.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 50, 
            Date = startOfMonth.AddDays(5),
            IsDeleted = false
        });

        // Act
        var result = category.BudgetUtilizationPercentage;

        // Assert
        Assert.Equal(25, result); // 50 / 200 * 100 = 25%
    }
}