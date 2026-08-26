using FinTrack.Core.Entities;

namespace FinTrack.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Goal entity
/// </summary>
public class GoalTests
{
    [Fact]
    public void Constructor_ShouldInitializeDefaultValues()
    {
        // Arrange & Act
        var goal = new Goal();

        // Assert
        Assert.Empty(goal.Name);
        Assert.Empty(goal.Description);
        Assert.Equal(0, goal.TargetAmount);
        Assert.Equal(0, goal.CurrentAmount);
        Assert.Equal(default(DateTime), goal.TargetDate);
        Assert.Equal(3, goal.Priority);
        Assert.Empty(goal.Category);
        Assert.Equal("#3B82F6", goal.Color);
        Assert.False(goal.IsCompleted);
        Assert.Null(goal.CompletedDate);
        Assert.NotNull(goal.Milestones);
        Assert.Empty(goal.Milestones);
    }

    [Theory]
    [InlineData(1000, 250, 25)]
    [InlineData(500, 500, 100)]
    [InlineData(1000, 1200, 100)] // Should cap at 100%
    [InlineData(0, 100, 0)] // Zero target should return 0
    public void ProgressPercentage_ShouldCalculateCorrectly(decimal target, decimal current, decimal expected)
    {
        // Arrange
        var goal = new Goal
        {
            TargetAmount = target,
            CurrentAmount = current
        };

        // Act
        var result = goal.ProgressPercentage;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1000, 250, 750)]
    [InlineData(500, 500, 0)]
    [InlineData(1000, 1200, 0)] // Should not go negative
    public void RemainingAmount_ShouldCalculateCorrectly(decimal target, decimal current, decimal expected)
    {
        // Arrange
        var goal = new Goal
        {
            TargetAmount = target,
            CurrentAmount = current
        };

        // Act
        var result = goal.RemainingAmount;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DaysRemaining_WithFutureDate_ShouldReturnPositiveDays()
    {
        // Arrange
        var goal = new Goal
        {
            TargetDate = DateTime.Now.AddDays(30)
        };

        // Act
        var result = goal.DaysRemaining;

        // Assert
        Assert.True(result >= 29 && result <= 30); // Account for timing differences
    }

    [Fact]
    public void DaysRemaining_WithPastDate_ShouldReturnZero()
    {
        // Arrange
        var goal = new Goal
        {
            TargetDate = DateTime.Now.AddDays(-10)
        };

        // Act
        var result = goal.DaysRemaining;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void IsOverdue_WithPastDateAndNotCompleted_ShouldReturnTrue()
    {
        // Arrange
        var goal = new Goal
        {
            TargetDate = DateTime.Now.AddDays(-5),
            IsCompleted = false
        };

        // Act
        var result = goal.IsOverdue;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOverdue_WithPastDateButCompleted_ShouldReturnFalse()
    {
        // Arrange
        var goal = new Goal
        {
            TargetDate = DateTime.Now.AddDays(-5),
            IsCompleted = true
        };

        // Act
        var result = goal.IsOverdue;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOverdue_WithFutureDate_ShouldReturnFalse()
    {
        // Arrange
        var goal = new Goal
        {
            TargetDate = DateTime.Now.AddDays(10),
            IsCompleted = false
        };

        // Act
        var result = goal.IsOverdue;

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(1000, 250, 30, 750)] // Need $750 over 30 days = $750/month (1 month)
    [InlineData(600, 100, 60, 254.24)] // Need $500 over 60 days = $254.24/month (60/30 = 2 months)
    [InlineData(1000, 1000, 30, 0)] // Already completed
    public void RequiredMonthlySavings_ShouldCalculateCorrectly(decimal target, decimal current, int daysRemaining, decimal expectedMonthly)
    {
        // Arrange
        var goal = new Goal
        {
            TargetAmount = target,
            CurrentAmount = current,
            TargetDate = DateTime.Now.AddDays(daysRemaining)
        };

        // Act
        var result = goal.RequiredMonthlySavings;

        // Assert
        Assert.Equal(expectedMonthly, Math.Round(result, 2));
    }

    [Fact]
    public void RequiredMonthlySavings_WithOverdueGoal_ShouldReturnZero()
    {
        // Arrange
        var goal = new Goal
        {
            TargetAmount = 1000,
            CurrentAmount = 250,
            TargetDate = DateTime.Now.AddDays(-10) // Overdue
        };

        // Act
        var result = goal.RequiredMonthlySavings;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void UpdateProgress_ShouldUpdateCurrentAmountAndMarkAsModified()
    {
        // Arrange
        var goal = new Goal
        {
            TargetAmount = 1000,
            CurrentAmount = 250
        };
        goal.MarkAsSynced(); // Set to synced to test modification tracking
        var originalVersion = goal.Version;

        // Act
        goal.UpdateProgress(400);

        // Assert
        Assert.Equal(400, goal.CurrentAmount);
        Assert.Equal(originalVersion + 1, goal.Version);
    }

    [Fact]
    public void UpdateProgress_WhenReachingTarget_ShouldMarkAsCompleted()
    {
        // Arrange
        var goal = new Goal
        {
            TargetAmount = 1000,
            CurrentAmount = 900,
            IsCompleted = false
        };
        var beforeUpdate = DateTime.UtcNow;

        // Act
        goal.UpdateProgress(1000);

        // Assert
        Assert.True(goal.IsCompleted);
        Assert.NotNull(goal.CompletedDate);
        Assert.True(goal.CompletedDate >= beforeUpdate);
        Assert.True(goal.CompletedDate <= DateTime.UtcNow);
    }

    [Fact]
    public void UpdateProgress_WhenExceedingTarget_ShouldMarkAsCompleted()
    {
        // Arrange
        var goal = new Goal
        {
            TargetAmount = 1000,
            CurrentAmount = 900,
            IsCompleted = false
        };

        // Act
        goal.UpdateProgress(1200);

        // Assert
        Assert.True(goal.IsCompleted);
        Assert.NotNull(goal.CompletedDate);
    }

    [Fact]
    public void UpdateProgress_ShouldCheckMilestoneAchievements()
    {
        // Arrange
        var goal = new Goal
        {
            TargetAmount = 1000,
            CurrentAmount = 200
        };
        
        var milestone1 = new GoalMilestone
        {
            Name = "25% Complete",
            TargetAmount = 250,
            IsAchieved = false
        };
        var milestone2 = new GoalMilestone
        {
            Name = "50% Complete",
            TargetAmount = 500,
            IsAchieved = false
        };
        
        goal.Milestones.Add(milestone1);
        goal.Milestones.Add(milestone2);

        // Act
        goal.UpdateProgress(300);

        // Assert
        Assert.True(milestone1.IsAchieved);
        Assert.False(milestone2.IsAchieved); // Should not be achieved yet
    }

    [Fact]
    public void AddMilestone_ShouldCreateAndAddMilestone()
    {
        // Arrange
        var goal = new Goal { Id = 1 };
        goal.MarkAsSynced(); // Set to synced to test modification tracking
        var originalVersion = goal.Version;

        // Act
        goal.AddMilestone("25% Complete", 250, "Quarter way there!");

        // Assert
        Assert.Single(goal.Milestones);
        var milestone = goal.Milestones.First();
        Assert.Equal(1, milestone.GoalId);
        Assert.Equal("25% Complete", milestone.Name);
        Assert.Equal(250, milestone.TargetAmount);
        Assert.Equal("Quarter way there!", milestone.Description);
        Assert.False(milestone.IsAchieved);
        Assert.Equal(originalVersion + 1, goal.Version);
    }
}