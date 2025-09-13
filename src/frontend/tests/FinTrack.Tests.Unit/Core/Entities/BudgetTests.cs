using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Tests.Unit.Core.Entities;

/// <summary>
/// Unit tests for Budget entity
/// </summary>
public class BudgetTests
{
    [Fact]
    public void Constructor_ShouldInitializeDefaultValues()
    {
        // Arrange & Act
        var budget = new Budget();

        // Assert
        Assert.Equal(BudgetPeriod.Monthly, budget.Period);
        Assert.Equal(DateTime.Today, budget.StartDate);
        Assert.Equal(DateTime.Today.AddMonths(1).AddDays(-1), budget.EndDate);
        Assert.True(budget.IsActive);
        Assert.Equal(80, budget.AlertThreshold);
        Assert.Equal("#3B82F6", budget.Color);
        Assert.True(budget.AlertsEnabled);
        Assert.False(budget.RolloverEnabled);
        Assert.Equal(0, budget.SpentAmount);
        Assert.Equal(0, budget.RolloverAmount);
        Assert.Empty(budget.Name);
        Assert.Null(budget.Description);
        Assert.Null(budget.CategoryId);
    }

    [Theory]
    [InlineData(1000, 0, 1000)]
    [InlineData(1000, 200, 1200)]
    [InlineData(500, 100, 600)]
    public void TotalBudgetAmount_ShouldIncludeRollover(decimal amount, decimal rollover, decimal expected)
    {
        // Arrange
        var budget = new Budget 
        { 
            Amount = amount, 
            RolloverAmount = rollover 
        };

        // Act
        var result = budget.TotalBudgetAmount;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1000, 0, 300, 700)]
    [InlineData(1000, 200, 500, 700)]
    [InlineData(1000, 0, 1200, 0)] // Overspent, should return 0
    public void RemainingAmount_ShouldCalculateCorrectly(decimal amount, decimal rollover, decimal spent, decimal expected)
    {
        // Arrange
        var budget = new Budget 
        { 
            Amount = amount, 
            RolloverAmount = rollover,
            SpentAmount = spent
        };

        // Act
        var result = budget.RemainingAmount;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1000, 0, 250, 25)]
    [InlineData(1000, 200, 600, 50)]
    [InlineData(1000, 0, 0, 0)]
    [InlineData(0, 0, 100, 0)] // Zero budget should return 0
    public void UtilizationPercentage_ShouldCalculateCorrectly(decimal amount, decimal rollover, decimal spent, decimal expected)
    {
        // Arrange
        var budget = new Budget 
        { 
            Amount = amount, 
            RolloverAmount = rollover,
            SpentAmount = spent
        };

        // Act
        var result = budget.UtilizationPercentage;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1000, 0, 500, false)]
    [InlineData(1000, 0, 1000, false)] // Exactly at limit
    [InlineData(1000, 0, 1001, true)]
    [InlineData(1000, 200, 1200, false)] // At limit with rollover
    [InlineData(1000, 200, 1201, true)] // Exceeded with rollover
    public void IsExceeded_ShouldCalculateCorrectly(decimal amount, decimal rollover, decimal spent, bool expected)
    {
        // Arrange
        var budget = new Budget 
        { 
            Amount = amount, 
            RolloverAmount = rollover,
            SpentAmount = spent
        };

        // Act
        var result = budget.IsExceeded;

        // Assert
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> AlertThresholdTestData =>
        new List<object[]>
        {
            new object[] { 1000m, 0m, 500m, 80m, false }, // 50% utilization, 80% threshold
            new object[] { 1000m, 0m, 800m, 80m, true }, // 80% utilization, 80% threshold
            new object[] { 1000m, 0m, 900m, 80m, true }, // 90% utilization, 80% threshold
            new object[] { 1000m, 0m, 500m, null, false } // No threshold set
        };

    [Theory]
    [MemberData(nameof(AlertThresholdTestData))]
    public void HasReachedAlertThreshold_ShouldCalculateCorrectly(decimal amount, decimal rollover, decimal spent, decimal? threshold, bool expected)
    {
        // Arrange
        var budget = new Budget 
        { 
            Amount = amount, 
            RolloverAmount = rollover,
            SpentAmount = spent,
            AlertThreshold = threshold
        };

        // Act
        var result = budget.HasReachedAlertThreshold;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DaysRemaining_WithFutureDates_ShouldCalculateCorrectly()
    {
        // Arrange
        var budget = new Budget 
        { 
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(10)
        };

        // Act
        var result = budget.DaysRemaining;

        // Assert
        Assert.Equal(11, result); // Today + 10 more days
    }

    [Fact]
    public void DaysRemaining_WithPastDates_ShouldReturnZero()
    {
        // Arrange
        var budget = new Budget 
        { 
            StartDate = DateTime.Today.AddDays(-20),
            EndDate = DateTime.Today.AddDays(-10)
        };

        // Act
        var result = budget.DaysRemaining;

        // Assert
        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData(0, 0, true)] // Today is start and end date
    [InlineData(-5, 5, true)] // Started 5 days ago, ends in 5 days
    [InlineData(1, 10, false)] // Starts tomorrow
    [InlineData(-10, -1, false)] // Ended yesterday
    public void IsCurrentPeriod_ShouldCalculateCorrectly(int startDaysOffset, int endDaysOffset, bool expected)
    {
        // Arrange
        var budget = new Budget 
        { 
            StartDate = DateTime.Today.AddDays(startDaysOffset),
            EndDate = DateTime.Today.AddDays(endDaysOffset)
        };

        // Act
        var result = budget.IsCurrentPeriod;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DailySpendingRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var budget = new Budget 
        { 
            StartDate = DateTime.Today.AddDays(-9), // 10 days ago (including today = 10 days)
            SpentAmount = 100
        };

        // Act
        var result = budget.DailySpendingRate;

        // Assert
        Assert.Equal(10, result); // 100 / 10 days = 10 per day
    }

    [Fact]
    public void ProjectedSpending_ForCurrentPeriod_ShouldCalculateCorrectly()
    {
        // Arrange
        var budget = new Budget 
        { 
            StartDate = DateTime.Today.AddDays(-4), // 5 days total (including today)
            EndDate = DateTime.Today.AddDays(5), // 10 days total
            SpentAmount = 50
        };

        // Act
        var result = budget.ProjectedSpending;

        // Assert
        Assert.Equal(100, result); // (50 / 5) * 10 = 100
    }

    [Fact]
    public void ProjectedSpending_ForPastPeriod_ShouldReturnSpentAmount()
    {
        // Arrange
        var budget = new Budget 
        { 
            StartDate = DateTime.Today.AddDays(-20),
            EndDate = DateTime.Today.AddDays(-10),
            SpentAmount = 500
        };

        // Act
        var result = budget.ProjectedSpending;

        // Assert
        Assert.Equal(500, result);
    }

    [Fact]
    public void AddSpending_ShouldUpdateSpentAmountAndMarkAsModified()
    {
        // Arrange
        var budget = new Budget { SpentAmount = 100 };
        var originalVersion = budget.Version;
        var originalUpdatedAt = budget.UpdatedAt;
        
        Thread.Sleep(1);

        // Act
        budget.AddSpending(50);

        // Assert
        Assert.Equal(150, budget.SpentAmount);
        Assert.Equal(originalVersion + 1, budget.Version);
        Assert.True(budget.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void AddSpending_WithNegativeAmount_ShouldThrowException()
    {
        // Arrange
        var budget = new Budget();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => budget.AddSpending(-50));
        Assert.Equal("amount", exception.ParamName);
        Assert.Contains("cannot be negative", exception.Message);
    }

    [Fact]
    public void RemoveSpending_ShouldUpdateSpentAmountAndMarkAsModified()
    {
        // Arrange
        var budget = new Budget { SpentAmount = 100 };
        var originalVersion = budget.Version;
        var originalUpdatedAt = budget.UpdatedAt;
        
        Thread.Sleep(1);

        // Act
        budget.RemoveSpending(30);

        // Assert
        Assert.Equal(70, budget.SpentAmount);
        Assert.Equal(originalVersion + 1, budget.Version);
        Assert.True(budget.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void RemoveSpending_MoreThanSpent_ShouldSetToZero()
    {
        // Arrange
        var budget = new Budget { SpentAmount = 50 };

        // Act
        budget.RemoveSpending(100);

        // Assert
        Assert.Equal(0, budget.SpentAmount);
    }

    [Fact]
    public void RemoveSpending_WithNegativeAmount_ShouldThrowException()
    {
        // Arrange
        var budget = new Budget();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => budget.RemoveSpending(-50));
        Assert.Equal("amount", exception.ParamName);
        Assert.Contains("cannot be negative", exception.Message);
    }

    [Fact]
    public void ResetForNewPeriod_WithRolloverEnabled_ShouldCarryOverRemaining()
    {
        // Arrange
        var budget = new Budget 
        { 
            Amount = 1000,
            SpentAmount = 600,
            RolloverEnabled = true
        };
        var newStart = DateTime.Today.AddMonths(1);
        var newEnd = DateTime.Today.AddMonths(2);

        // Act
        budget.ResetForNewPeriod(newStart, newEnd);

        // Assert
        Assert.Equal(newStart, budget.StartDate);
        Assert.Equal(newEnd, budget.EndDate);
        Assert.Equal(0, budget.SpentAmount);
        Assert.Equal(400, budget.RolloverAmount); // 1000 - 600 = 400
    }

    [Fact]
    public void ResetForNewPeriod_WithRolloverDisabled_ShouldNotCarryOver()
    {
        // Arrange
        var budget = new Budget 
        { 
            Amount = 1000,
            SpentAmount = 600,
            RolloverEnabled = false
        };
        var newStart = DateTime.Today.AddMonths(1);
        var newEnd = DateTime.Today.AddMonths(2);

        // Act
        budget.ResetForNewPeriod(newStart, newEnd);

        // Assert
        Assert.Equal(newStart, budget.StartDate);
        Assert.Equal(newEnd, budget.EndDate);
        Assert.Equal(0, budget.SpentAmount);
        Assert.Equal(0, budget.RolloverAmount);
    }

    [Fact]
    public void IsValid_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var budget = new Budget
        {
            Name = "Groceries",
            Amount = 500,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1),
            AlertThreshold = 80,
            Color = "#FF0000"
        };

        // Act
        var result = budget.IsValid();

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
        var budget = new Budget 
        { 
            Name = name,
            Amount = 500,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1)
        };

        // Act
        var result = budget.IsValid();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void IsValid_WithInvalidAmount_ShouldReturnFalse(decimal amount)
    {
        // Arrange
        var budget = new Budget 
        { 
            Name = "Test",
            Amount = amount,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1)
        };

        // Act
        var result = budget.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithStartDateAfterEndDate_ShouldReturnFalse()
    {
        // Arrange
        var budget = new Budget 
        { 
            Name = "Test",
            Amount = 500,
            StartDate = DateTime.Today.AddDays(10),
            EndDate = DateTime.Today
        };

        // Act
        var result = budget.IsValid();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(110)]
    public void IsValid_WithInvalidAlertThreshold_ShouldReturnFalse(decimal threshold)
    {
        // Arrange
        var budget = new Budget 
        { 
            Name = "Test",
            Amount = 500,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1),
            AlertThreshold = threshold
        };

        // Act
        var result = budget.IsValid();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("#GG0000")]
    [InlineData("#FF00")]
    public void IsValid_WithInvalidColor_ShouldReturnFalse(string color)
    {
        // Arrange
        var budget = new Budget 
        { 
            Name = "Test",
            Amount = 500,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1),
            Color = color
        };

        // Act
        var result = budget.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithNegativeSpentAmount_ShouldReturnFalse()
    {
        // Arrange
        var budget = new Budget 
        { 
            Name = "Test",
            Amount = 500,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(1),
            SpentAmount = -100
        };

        // Act
        var result = budget.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CreateNextPeriodBudget_Monthly_ShouldCreateCorrectDates()
    {
        // Arrange
        var budget = new Budget 
        { 
            Name = "Groceries",
            Amount = 500,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 31),
            RolloverEnabled = true,
            SpentAmount = 300
        };

        // Act
        var nextBudget = budget.CreateNextPeriodBudget();

        // Assert
        Assert.Equal("Groceries", nextBudget.Name);
        Assert.Equal(500, nextBudget.Amount);
        Assert.Equal(BudgetPeriod.Monthly, nextBudget.Period);
        Assert.Equal(new DateTime(2024, 2, 1), nextBudget.StartDate);
        Assert.Equal(new DateTime(2024, 2, 29), nextBudget.EndDate); // 2024 is leap year
        Assert.Equal(200, nextBudget.RolloverAmount); // 500 - 300 = 200
        Assert.Equal(0, nextBudget.SpentAmount);
    }

    [Theory]
    [InlineData(BudgetPeriod.Weekly, 7)]
    [InlineData(BudgetPeriod.Quarterly, 90)] // Approximately 3 months
    [InlineData(BudgetPeriod.Annual, 365)]
    public void CreateNextPeriodBudget_DifferentPeriods_ShouldCalculateCorrectly(BudgetPeriod period, int expectedDaysApprox)
    {
        // Arrange
        var budget = new Budget 
        { 
            Name = "Test",
            Amount = 500,
            Period = period,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 31)
        };

        // Act
        var nextBudget = budget.CreateNextPeriodBudget();

        // Assert
        Assert.Equal(new DateTime(2024, 2, 1), nextBudget.StartDate);
        
        var actualDays = (nextBudget.EndDate - nextBudget.StartDate).Days + 1;
        Assert.True(Math.Abs(actualDays - expectedDaysApprox) <= 5); // Allow some variance for month lengths
    }
}