using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Tests.Unit.Domain;

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
        Assert.Empty(category.Name);
        Assert.Null(category.Description);
        Assert.Null(category.Icon);
        Assert.Null(category.ParentCategoryId);
        Assert.Null(category.BudgetLimit);
        Assert.NotNull(category.Transactions);
        Assert.NotNull(category.SubCategories);
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
    public void FullPath_WithParent_ShouldReturnFullHierarchy()
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
    public void FullPath_WithMultipleLevels_ShouldReturnCompleteHierarchy()
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
    public void Level_WithOneParent_ShouldReturnOne()
    {
        // Arrange
        var parent = new Category { Name = "Food" };
        var child = new Category 
        { 
            Name = "Restaurants",
            ParentCategory = parent
        };

        // Act
        var result = child.Level;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Level_WithMultipleParents_ShouldReturnCorrectDepth()
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
    public void HasSubCategories_WithOnlyDeletedSubCategories_ShouldReturnFalse()
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
        var activeCategory = new Category { Name = "Active", IsActive = true, IsDeleted = false };
        var inactiveCategory = new Category { Name = "Inactive", IsActive = false, IsDeleted = false };
        var deletedCategory = new Category { Name = "Deleted", IsActive = true, IsDeleted = true };
        
        category.SubCategories.Add(activeCategory);
        category.SubCategories.Add(inactiveCategory);
        category.SubCategories.Add(deletedCategory);

        // Act
        var result = category.ActiveSubCategories.ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Active", result[0].Name);
    }

    [Fact]
    public void ActiveTransactions_ShouldReturnOnlyNonDeletedTransactions()
    {
        // Arrange
        var category = new Category();
        var activeTransaction = new Transaction { Description = "Active", IsDeleted = false };
        var deletedTransaction = new Transaction { Description = "Deleted", IsDeleted = true };
        
        category.Transactions.Add(activeTransaction);
        category.Transactions.Add(deletedTransaction);

        // Act
        var result = category.ActiveTransactions.ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Active", result[0].Description);
    }

    [Fact]
    public void IsValid_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var category = new Category
        {
            Name = "Food",
            Color = "#FF5722",
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
    [InlineData("#FF5722", true)] // Valid hex color
    [InlineData("#000000", true)] // Valid black
    [InlineData("#FFFFFF", true)] // Valid white
    [InlineData("FF5722", false)] // Missing #
    [InlineData("#FF572", false)] // Too short
    [InlineData("#FF57222", false)] // Too long
    [InlineData("#GG5722", false)] // Invalid characters
    [InlineData("", true)] // Empty string is valid (uses default)
    public void IsValid_WithVariousColors_ShouldValidateCorrectly(string color, bool expected)
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
        Assert.Equal(expected, result);
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
            Date = new DateTime(2024, 1, 20),
            IsDeleted = false 
        });
        category.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Income, 
            Amount = 200, 
            Date = new DateTime(2024, 1, 10),
            IsDeleted = false 
        }); // Should not be included
        category.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 75, 
            Date = new DateTime(2024, 2, 1),
            IsDeleted = false 
        }); // Outside date range
        category.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 25, 
            Date = new DateTime(2024, 1, 25),
            IsDeleted = true 
        }); // Deleted transaction

        // Act
        var result = category.CalculateSpending(startDate, endDate);

        // Assert
        Assert.Equal(150, result);
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

        var subSubCategory = new Category { IsActive = true, IsDeleted = false };
        subSubCategory.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 25, 
            Date = new DateTime(2024, 1, 25),
            IsDeleted = false 
        });

        subCategory.SubCategories.Add(subSubCategory);
        parentCategory.SubCategories.Add(subCategory);

        // Act
        var result = parentCategory.CalculateSpendingWithSubCategories(startDate, endDate);

        // Assert
        Assert.Equal(175, result); // 100 + 50 + 25
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
        var category = new Category { BudgetLimit = 1000 };
        
        // Add transactions for current month
        var currentMonth = DateTime.Today;
        var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        
        category.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 250, 
            Date = startOfMonth.AddDays(5),
            IsDeleted = false 
        });
        category.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 150, 
            Date = startOfMonth.AddDays(15),
            IsDeleted = false 
        });

        // Act
        var result = category.BudgetUtilizationPercentage;

        // Assert
        Assert.Equal(40, result); // (250 + 150) / 1000 * 100 = 40%
    }
}