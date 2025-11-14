using FinTrack.Core.Entities;
using FinTrack.Tests.Unit.Helpers;

namespace FinTrack.Tests.Unit.Domain;

/// <summary>
/// Comprehensive unit tests for Goal entity and GoalMilestone
/// </summary>
public class GoalEntityTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var goal = new Goal();

        // Assert
        Assert.Equal(string.Empty, goal.Name);
        Assert.Equal(string.Empty, goal.Description);
        Assert.Equal(0m, goal.TargetAmount);
        Assert.Equal(0m, goal.CurrentAmount);
        Assert.Equal(3, goal.Priority);
        Assert.Equal(string.Empty, goal.Category);
        Assert.Equal("#3B82F6", goal.Color);
        Assert.False(goal.IsCompleted);
        Assert.Null(goal.CompletedDate);
        Assert.Empty(goal.Milestones);
    }

    [Theory]
    [InlineData(0, 0, 0)] // No progress
    [InlineData(100, 25, 25)] // 25% progress
    [InlineData(100, 50, 50)] // 50% progress
    [InlineData(100, 100, 100)] // 100% progress
    [InlineData(100, 150, 100)] // Over 100% should cap at 100%
    [InlineData(0, 50, 0)] // Zero target should return 0%
    public void ProgressPercentage_WithDifferentAmounts_ShouldReturnCorrectPercentage(
        decimal targetAmount, decimal currentAmount, decimal expectedPercentage)
    {
        // Arrange
        var goal = TestDataBuilder.Goal()
            .WithTargetAmount(targetAmount)
            .WithCurrentAmount(currentAmount)
            .Build();

        // Act
        var progressPercentage = goal.ProgressPercentage;

        // Assert
        Assert.Equal(expectedPercentage, progressPercentage);
    }

    [Theory]
    [InlineData(1000, 300, 700)] // Normal case
    [InlineData(1000, 1000, 0)] // Fully achieved
    [InlineData(1000, 1200, 0)] // Over-achieved should return 0
    [InlineData(0, 100, 0)] // Zero target
    public void RemainingAmount_WithDifferentAmounts_ShouldReturnCorrectRemaining(
        decimal targetAmount, decimal currentAmount, decimal expectedRemaining)
    {
        // Arrange
        var goal = TestDataBuilder.Goal()
            .WithTargetAmount(targetAmount)
            .WithCurrentAmount(currentAmount)
            .Build();

        // Act
        var remainingAmount = goal.RemainingAmount;

        // Assert
        Assert.Equal(expectedRemaining, remainingAmount);
    }

    [Fact]
    public void DaysRemaining_WithFutureTargetDate_ShouldReturnPositiveDays()
    {
        // Arrange
        var futureDate = DateTime.Now.AddDays(30);
        var goal = TestDataBuilder.Goal()
            .WithTargetDate(futureDate)
            .Build();

        // Act
        var daysRemaining = goal.DaysRemaining;

        // Assert
        Assert.True(daysRemaining >= 29 && daysRemaining <= 30); // Account for timing differences
    }

    [Fact]
    public void DaysRemaining_WithPastTargetDate_ShouldReturnZero()
    {
        // Arrange
        var pastDate = DateTime.Now.AddDays(-10);
        var goal = TestDataBuilder.Goal()
            .WithTargetDate(pastDate)
            .Build();

        // Act
        var daysRemaining = goal.DaysRemaining;

        // Assert
        Assert.Equal(0, daysRemaining);
    }

    [Fact]
    public void IsOverdue_WithPastTargetDateAndNotCompleted_ShouldReturnTrue()
    {
        // Arrange
        var pastDate = DateTime.Now.AddDays(-5);
        var goal = TestDataBuilder.Goal()
            .WithTargetDate(pastDate)
            .Build();

        // Act
        var isOverdue = goal.IsOverdue;

        // Assert
        Assert.True(isOverdue);
    }

    [Fact]
    public void IsOverdue_WithPastTargetDateButCompleted_ShouldReturnFalse()
    {
        // Arrange
        var pastDate = DateTime.Now.AddDays(-5);
        var goal = TestDataBuilder.Goal()
            .WithTargetDate(pastDate)
            .AsCompleted()
            .Build();

        // Act
        var isOverdue = goal.IsOverdue;

        // Assert
        Assert.False(isOverdue);
    }

    [Fact]
    public void IsOverdue_WithFutureTargetDate_ShouldReturnFalse()
    {
        // Arrange
        var futureDate = DateTime.Now.AddDays(10);
        var goal = TestDataBuilder.Goal()
            .WithTargetDate(futureDate)
            .Build();

        // Act
        var isOverdue = goal.IsOverdue;

        // Assert
        Assert.False(isOverdue);
    }

    [Theory]
    [InlineData(1000, 400, 30, 600)] // Normal case: 600 remaining / 1 month = 600 per month
    [InlineData(1000, 1000, 30, 0)] // Already completed
    [InlineData(1000, 400, 0, 0)] // No days remaining, returns 0
    [InlineData(1000, 400, -10, 0)] // Past due, returns 0
    public void RequiredMonthlySavings_WithDifferentScenarios_ShouldReturnCorrectAmount(
        decimal targetAmount, decimal currentAmount, int daysFromNow, decimal expectedMonthlySavings)
    {
        // Arrange
        var targetDate = DateTime.Now.AddDays(daysFromNow);
        var goal = TestDataBuilder.Goal()
            .WithTargetAmount(targetAmount)
            .WithCurrentAmount(currentAmount)
            .WithTargetDate(targetDate)
            .Build();

        // Act
        var requiredMonthlySavings = goal.RequiredMonthlySavings;

        // Assert
        Assert.Equal(expectedMonthlySavings, requiredMonthlySavings, 2); // Allow for small rounding differences
    }

    [Fact]
    public void UpdateProgress_WithAmountBelowTarget_ShouldUpdateCurrentAmountOnly()
    {
        // Arrange
        var goal = TestDataBuilder.Goal()
            .WithTargetAmount(1000m)
            .WithCurrentAmount(200m)
            .Build();

        // Act
        goal.UpdateProgress(500m);

        // Assert
        Assert.Equal(500m, goal.CurrentAmount);
        Assert.False(goal.IsCompleted);
        Assert.Null(goal.CompletedDate);
    }

    [Fact]
    public void UpdateProgress_WithAmountReachingTarget_ShouldMarkAsCompleted()
    {
        // Arrange
        var goal = TestDataBuilder.Goal()
            .WithTargetAmount(1000m)
            .WithCurrentAmount(800m)
            .Build();

        var beforeUpdate = DateTime.UtcNow;

        // Act
        goal.UpdateProgress(1000m);

        // Assert
        Assert.Equal(1000m, goal.CurrentAmount);
        Assert.True(goal.IsCompleted);
        Assert.NotNull(goal.CompletedDate);
        Assert.True(goal.CompletedDate >= beforeUpdate);
        Assert.True(goal.CompletedDate <= DateTime.UtcNow);
    }

    [Fact]
    public void UpdateProgress_WithAmountExceedingTarget_ShouldMarkAsCompleted()
    {
        // Arrange
        var goal = TestDataBuilder.Goal()
            .WithTargetAmount(1000m)
            .WithCurrentAmount(800m)
            .Build();

        // Act
        goal.UpdateProgress(1200m);

        // Assert
        Assert.Equal(1200m, goal.CurrentAmount);
        Assert.True(goal.IsCompleted);
        Assert.NotNull(goal.CompletedDate);
    }

    [Fact]
    public void UpdateProgress_ShouldMarkMilestonesAsAchieved()
    {
        // Arrange
        var goal = TestDataBuilder.Goal()
            .WithTargetAmount(1000m)
            .WithCurrentAmount(200m)
            .WithMilestone("25% Complete", 250m, "Quarter way there!")
            .WithMilestone("50% Complete", 500m, "Halfway there!")
            .WithMilestone("75% Complete", 750m, "Almost there!")
            .Build();

        // Act
        goal.UpdateProgress(600m);

        // Assert
        Assert.Equal(600m, goal.CurrentAmount);
        
        // Check milestones
        var milestones = goal.Milestones.ToList();
        Assert.True(milestones[0].IsAchieved); // 25% milestone should be achieved
        Assert.True(milestones[1].IsAchieved); // 50% milestone should be achieved
        Assert.False(milestones[2].IsAchieved); // 75% milestone should not be achieved yet
    }

    [Fact]
    public void UpdateProgress_WithAlreadyAchievedMilestones_ShouldNotChangeTheirStatus()
    {
        // Arrange
        var goal = TestDataBuilder.Goal()
            .WithTargetAmount(1000m)
            .WithCurrentAmount(600m)
            .WithMilestone("50% Complete", 500m)
            .Build();

        // Manually mark milestone as achieved
        goal.Milestones[0].MarkAsAchieved();
        var originalAchievedDate = goal.Milestones[0].AchievedDate;

        // Act
        goal.UpdateProgress(700m);

        // Assert
        Assert.True(goal.Milestones[0].IsAchieved);
        Assert.Equal(originalAchievedDate, goal.Milestones[0].AchievedDate); // Should not change
    }

    [Fact]
    public void AddMilestone_ShouldCreateAndAddMilestone()
    {
        // Arrange
        var goal = TestDataBuilder.Goal()
            .WithId(1)
            .Build();

        // Act
        goal.AddMilestone("Test Milestone", 500m, "Test description");

        // Assert
        Assert.Single(goal.Milestones);
        var milestone = goal.Milestones[0];
        Assert.Equal(1, milestone.GoalId);
        Assert.Equal("Test Milestone", milestone.Name);
        Assert.Equal(500m, milestone.TargetAmount);
        Assert.Equal("Test description", milestone.Description);
        Assert.False(milestone.IsAchieved);
        Assert.Null(milestone.AchievedDate);
    }

    [Fact]
    public void AddMilestone_WithoutDescription_ShouldUseEmptyString()
    {
        // Arrange
        var goal = TestDataBuilder.Goal().Build();

        // Act
        goal.AddMilestone("Test Milestone", 500m);

        // Assert
        Assert.Single(goal.Milestones);
        Assert.Equal(string.Empty, goal.Milestones[0].Description);
    }
}

/// <summary>
/// Unit tests for GoalMilestone entity
/// </summary>
public class GoalMilestoneTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var milestone = new GoalMilestone();

        // Assert
        Assert.Equal(0, milestone.GoalId);
        Assert.Equal(string.Empty, milestone.Name);
        Assert.Equal(string.Empty, milestone.Description);
        Assert.Equal(0m, milestone.TargetAmount);
        Assert.False(milestone.IsAchieved);
        Assert.Null(milestone.AchievedDate);
    }

    [Fact]
    public void MarkAsAchieved_ShouldSetAchievedStatusAndDate()
    {
        // Arrange
        var milestone = new GoalMilestone
        {
            Name = "Test Milestone",
            TargetAmount = 500m
        };

        var beforeAchievement = DateTime.UtcNow;

        // Act
        milestone.MarkAsAchieved();

        // Assert
        Assert.True(milestone.IsAchieved);
        Assert.NotNull(milestone.AchievedDate);
        Assert.True(milestone.AchievedDate >= beforeAchievement);
        Assert.True(milestone.AchievedDate <= DateTime.UtcNow);
    }

    [Fact]
    public void MarkAsAchieved_CalledMultipleTimes_ShouldNotChangeOriginalDate()
    {
        // Arrange
        var milestone = new GoalMilestone
        {
            Name = "Test Milestone",
            TargetAmount = 500m
        };

        // Act
        milestone.MarkAsAchieved();
        var firstAchievedDate = milestone.AchievedDate;

        // Wait a sufficient amount to ensure time difference
        Thread.Sleep(100);
        
        milestone.MarkAsAchieved();

        // Assert
        Assert.Equal(firstAchievedDate, milestone.AchievedDate);
    }
}