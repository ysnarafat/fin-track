using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using Xunit;

namespace FinTrack.Tests.Unit.Domain;

public class CategoryTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var category = new Category();
        
        // Assert
        Assert.True(category.IsActive);
        Assert.False(category.IsSystem);
        Assert.Equal(0, category.SortOrder);
        Assert.Equal("#6B7280", category.Color);
        Assert.Equal(SyncStatus.PendingCreate, category.SyncStatus);
        Assert.NotEmpty(category.SyncId);
    }
    
    [Fact]
    public void FullPath_ReturnsNameOnly_WhenNoParent()
    {
        // Arrange
        var category = new Category { Name = "Food" };
        
        // Act & Assert
        Assert.Equal("Food", category.FullPath);
    }
    
    [Fact]
    public void FullPath_ReturnsFullPath_WhenHasParent()
    {
        // Arrange
        var parentCategory = new Category { Name = "Food" };
        var childCategory = new Category 
        { 
            Name = "Restaurants",
            ParentCategory = parentCategory
        };
        
        // Act & Assert
        Assert.Equal("Food > Restaurants", childCategory.FullPath);
    }
    
    [Fact]
    public void Level_ReturnsZero_ForRootCategory()
    {
        // Arrange
        var category = new Category { Name = "Food" };
        
        // Act & Assert
        Assert.Equal(0, category.Level);
    }
    
    [Fact]
    public void Level_ReturnsCorrectDepth_ForNestedCategory()
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
        
        // Act & Assert
        Assert.Equal(2, child.Level);
    }
    
    [Fact]
    public void HasSubCategories_ReturnsFalse_WhenNoSubCategories()
    {
        // Arrange
        var category = new Category();
        
        // Act & Assert
        Assert.False(category.HasSubCategories);
    }
    
    [Fact]
    public void HasSubCategories_ReturnsTrue_WhenHasActiveSubCategories()
    {
        // Arrange
        var category = new Category();
        category.SubCategories.Add(new Category { IsActive = true });
        
        // Act & Assert
        Assert.True(category.HasSubCategories);
    }    
    [
Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Valid Category", true)]
    public void IsValid_ValidatesName(string name, bool expected)
    {
        // Arrange
        var category = new Category { Name = name };
        
        // Act & Assert
        Assert.Equal(expected, category.IsValid());
    }
    
    [Fact]
    public void IsValid_FailsWhenParentIsItself()
    {
        // Arrange
        var category = new Category 
        { 
            Id = 1,
            Name = "Test",
            ParentCategoryId = 1
        };
        
        // Act & Assert
        Assert.False(category.IsValid());
    }
    
    [Theory]
    [InlineData("#FF0000", true)]
    [InlineData("#123456", true)]
    [InlineData("#ABCDEF", true)]
    [InlineData("FF0000", false)]
    [InlineData("#FF00", false)]
    [InlineData("#GG0000", false)]
    [InlineData("", true)] // Empty is valid (will use default)
    public void IsValid_ValidatesHexColor(string color, bool expected)
    {
        // Arrange
        var category = new Category 
        { 
            Name = "Test",
            Color = color
        };
        
        // Act & Assert
        Assert.Equal(expected, category.IsValid());
    }
    
    [Fact]
    public void CalculateSpending_ReturnsCorrectSum()
    {
        // Arrange
        var category = new Category();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        
        category.Transactions.Add(new Transaction 
        { 
            Amount = 100m, 
            Type = TransactionType.Expense, 
            Date = new DateTime(2024, 1, 15) 
        });
        category.Transactions.Add(new Transaction 
        { 
            Amount = 50m, 
            Type = TransactionType.Expense, 
            Date = new DateTime(2024, 1, 20) 
        });
        category.Transactions.Add(new Transaction 
        { 
            Amount = 200m, 
            Type = TransactionType.Income, 
            Date = new DateTime(2024, 1, 10) 
        });
        
        // Act
        var spending = category.CalculateSpending(startDate, endDate);
        
        // Assert
        Assert.Equal(150m, spending);
    }
    
    [Fact]
    public void BudgetUtilizationPercentage_ReturnsNull_WhenNoBudgetLimit()
    {
        // Arrange
        var category = new Category { BudgetLimit = null };
        
        // Act & Assert
        Assert.Null(category.BudgetUtilizationPercentage);
    }
    
    [Fact]
    public void BudgetUtilizationPercentage_ReturnsCorrectPercentage()
    {
        // Arrange
        var category = new Category { BudgetLimit = 1000m };
        var currentMonth = DateTime.Today;
        
        category.Transactions.Add(new Transaction 
        { 
            Amount = 250m, 
            Type = TransactionType.Expense, 
            Date = currentMonth
        });
        
        // Act
        var utilization = category.BudgetUtilizationPercentage;
        
        // Assert
        Assert.Equal(25m, utilization);
    }
    
    [Fact]
    public void ActiveSubCategories_ReturnsOnlyActiveNonDeletedCategories()
    {
        // Arrange
        var category = new Category();
        category.SubCategories.Add(new Category { IsActive = true, IsDeleted = false });
        category.SubCategories.Add(new Category { IsActive = false, IsDeleted = false });
        category.SubCategories.Add(new Category { IsActive = true, IsDeleted = true });
        
        // Act
        var activeSubCategories = category.ActiveSubCategories.ToList();
        
        // Assert
        Assert.Single(activeSubCategories);
        Assert.True(activeSubCategories.First().IsActive);
        Assert.False(activeSubCategories.First().IsDeleted);
    }
}