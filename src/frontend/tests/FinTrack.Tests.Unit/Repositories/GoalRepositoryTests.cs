using FinTrack.Core.Entities;
using FinTrack.Core.Interfaces;
using FinTrack.Tests.Unit.Helpers;
using Moq;

namespace FinTrack.Tests.Unit.Repositories;

/// <summary>
/// Unit tests for IGoalRepository interface implementations
/// </summary>
public class GoalRepositoryTests
{
    private readonly Mock<IGoalRepository> _mockRepository;

    public GoalRepositoryTests()
    {
        _mockRepository = new Mock<IGoalRepository>();
    }

    [Fact]
    public async Task GetGoalsByPriorityAsync_ShouldReturnGoalsOrderedByPriority()
    {
        // Arrange
        var goals = new List<Goal>
        {
            TestDataBuilder.Goal().WithName("High Priority").WithPriority(1).Build(),
            TestDataBuilder.Goal().WithName("Medium Priority").WithPriority(2).Build(),
            TestDataBuilder.Goal().WithName("Low Priority").WithPriority(3).Build()
        };

        _mockRepository.Setup(r => r.GetGoalsByPriorityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(goals);

        // Act
        var result = await _mockRepository.Object.GetGoalsByPriorityAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Equal("High Priority", result.First().Name);
        Assert.Equal(1, result.First().Priority);
        _mockRepository.Verify(r => r.GetGoalsByPriorityAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveGoalsAsync_ShouldReturnOnlyActiveGoals()
    {
        // Arrange
        var activeGoals = new List<Goal>
        {
            TestDataBuilder.Goal().WithName("Active Goal 1").WithTargetDate(DateTime.Now.AddMonths(6)).Build(),
            TestDataBuilder.Goal().WithName("Active Goal 2").WithTargetDate(DateTime.Now.AddMonths(12)).Build()
        };

        _mockRepository.Setup(r => r.GetActiveGoalsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeGoals);

        // Act
        var result = await _mockRepository.Object.GetActiveGoalsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, goal => 
        {
            Assert.False(goal.IsCompleted);
            Assert.False(goal.IsOverdue);
        });
        _mockRepository.Verify(r => r.GetActiveGoalsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCompletedGoalsAsync_ShouldReturnOnlyCompletedGoals()
    {
        // Arrange
        var completedGoals = new List<Goal>
        {
            TestDataBuilder.Goal().WithName("Completed Goal 1").AsCompleted().Build(),
            TestDataBuilder.Goal().WithName("Completed Goal 2").AsCompleted(DateTime.UtcNow.AddDays(-10)).Build()
        };

        _mockRepository.Setup(r => r.GetCompletedGoalsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedGoals);

        // Act
        var result = await _mockRepository.Object.GetCompletedGoalsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, goal => 
        {
            Assert.True(goal.IsCompleted);
            Assert.NotNull(goal.CompletedDate);
        });
        _mockRepository.Verify(r => r.GetCompletedGoalsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOverdueGoalsAsync_ShouldReturnOnlyOverdueGoals()
    {
        // Arrange
        var overdueGoals = new List<Goal>
        {
            TestDataBuilder.Goal()
                .WithName("Overdue Goal 1")
                .WithTargetDate(DateTime.Now.AddDays(-30))
                .WithTargetAmount(1000m)
                .WithCurrentAmount(500m)
                .Build(),
            TestDataBuilder.Goal()
                .WithName("Overdue Goal 2")
                .WithTargetDate(DateTime.Now.AddDays(-10))
                .WithTargetAmount(2000m)
                .WithCurrentAmount(1000m)
                .Build()
        };

        _mockRepository.Setup(r => r.GetOverdueGoalsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(overdueGoals);

        // Act
        var result = await _mockRepository.Object.GetOverdueGoalsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, goal => 
        {
            Assert.True(goal.IsOverdue);
            Assert.False(goal.IsCompleted);
        });
        _mockRepository.Verify(r => r.GetOverdueGoalsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalsByCategoryAsync_WithValidCategory_ShouldReturnMatchingGoals()
    {
        // Arrange
        var category = "Emergency Fund";
        var emergencyGoals = new List<Goal>
        {
            TestDataBuilder.Goal().WithName("Emergency Fund").WithCategory(category).Build(),
            TestDataBuilder.Goal().WithName("Backup Emergency Fund").WithCategory(category).Build()
        };

        _mockRepository.Setup(r => r.GetGoalsByCategoryAsync(category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emergencyGoals);

        // Act
        var result = await _mockRepository.Object.GetGoalsByCategoryAsync(category);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, goal => Assert.Equal(category, goal.Category));
        _mockRepository.Verify(r => r.GetGoalsByCategoryAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalsByCategoryAsync_WithNonExistentCategory_ShouldReturnEmptyList()
    {
        // Arrange
        var category = "NonExistent";

        _mockRepository.Setup(r => r.GetGoalsByCategoryAsync(category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Goal>());

        // Act
        var result = await _mockRepository.Object.GetGoalsByCategoryAsync(category);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockRepository.Verify(r => r.GetGoalsByCategoryAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalWithMilestonesAsync_WithValidGoalId_ShouldReturnGoalWithMilestones()
    {
        // Arrange
        var goalId = 1;
        var goal = TestDataBuilder.Goal()
            .WithId(goalId)
            .WithName("Goal with Milestones")
            .WithMilestone("25% Complete", 250m, "Quarter way there!")
            .WithMilestone("50% Complete", 500m, "Halfway there!")
            .WithMilestone("75% Complete", 750m, "Almost there!")
            .Build();

        _mockRepository.Setup(r => r.GetGoalWithMilestonesAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goal);

        // Act
        var result = await _mockRepository.Object.GetGoalWithMilestonesAsync(goalId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(goalId, result.Id);
        Assert.Equal(3, result.Milestones.Count);
        _mockRepository.Verify(r => r.GetGoalWithMilestonesAsync(goalId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalWithMilestonesAsync_WithInvalidGoalId_ShouldReturnNull()
    {
        // Arrange
        var goalId = 999;

        _mockRepository.Setup(r => r.GetGoalWithMilestonesAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal?)null);

        // Act
        var result = await _mockRepository.Object.GetGoalWithMilestonesAsync(goalId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetGoalWithMilestonesAsync(goalId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalsWithRecentMilestonesAsync_WithDefaultDays_ShouldReturnGoalsWithRecentAchievements()
    {
        // Arrange
        var goalsWithRecentMilestones = new List<Goal>
        {
            TestDataBuilder.Goal()
                .WithName("Goal with Recent Milestone")
                .WithMilestone("Recent Achievement", 500m)
                .Build()
        };

        // Mark milestone as recently achieved
        goalsWithRecentMilestones[0].Milestones[0].MarkAsAchieved();

        _mockRepository.Setup(r => r.GetGoalsWithRecentMilestonesAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goalsWithRecentMilestones);

        // Act
        var result = await _mockRepository.Object.GetGoalsWithRecentMilestonesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.First().Milestones.Any(m => m.IsAchieved));
        _mockRepository.Verify(r => r.GetGoalsWithRecentMilestonesAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalsWithRecentMilestonesAsync_WithCustomDays_ShouldUseSpecifiedTimeframe()
    {
        // Arrange
        var days = 30;
        var goalsWithRecentMilestones = new List<Goal>
        {
            TestDataBuilder.Goal()
                .WithName("Goal with Milestone in Last 30 Days")
                .WithMilestone("Monthly Achievement", 1000m)
                .Build()
        };

        _mockRepository.Setup(r => r.GetGoalsWithRecentMilestonesAsync(days, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goalsWithRecentMilestones);

        // Act
        var result = await _mockRepository.Object.GetGoalsWithRecentMilestonesAsync(days);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockRepository.Verify(r => r.GetGoalsWithRecentMilestonesAsync(days, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateGoalProgressAsync_WithValidGoalAndAmount_ShouldReturnUpdatedGoal()
    {
        // Arrange
        var goalId = 1;
        var newAmount = 750m;
        var updatedGoal = TestDataBuilder.Goal()
            .WithId(goalId)
            .WithName("Updated Goal")
            .WithTargetAmount(1000m)
            .WithCurrentAmount(newAmount)
            .WithMilestone("50% Complete", 500m)
            .WithMilestone("75% Complete", 750m)
            .Build();

        // Mark appropriate milestones as achieved
        updatedGoal.Milestones[0].MarkAsAchieved(); // 50% milestone
        updatedGoal.Milestones[1].MarkAsAchieved(); // 75% milestone

        _mockRepository.Setup(r => r.UpdateGoalProgressAsync(goalId, newAmount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedGoal);

        // Act
        var result = await _mockRepository.Object.UpdateGoalProgressAsync(goalId, newAmount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(goalId, result.Id);
        Assert.Equal(newAmount, result.CurrentAmount);
        Assert.Equal(2, result.Milestones.Count(m => m.IsAchieved));
        _mockRepository.Verify(r => r.UpdateGoalProgressAsync(goalId, newAmount, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateGoalProgressAsync_WithInvalidGoalId_ShouldReturnNull()
    {
        // Arrange
        var goalId = 999;
        var newAmount = 500m;

        _mockRepository.Setup(r => r.UpdateGoalProgressAsync(goalId, newAmount, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal?)null);

        // Act
        var result = await _mockRepository.Object.UpdateGoalProgressAsync(goalId, newAmount);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.UpdateGoalProgressAsync(goalId, newAmount, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateGoalProgressAsync_WithAmountReachingTarget_ShouldMarkGoalAsCompleted()
    {
        // Arrange
        var goalId = 1;
        var targetAmount = 1000m;
        var completedGoal = TestDataBuilder.Goal()
            .WithId(goalId)
            .WithName("Completed Goal")
            .WithTargetAmount(targetAmount)
            .WithCurrentAmount(targetAmount)
            .AsCompleted()
            .Build();

        _mockRepository.Setup(r => r.UpdateGoalProgressAsync(goalId, targetAmount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedGoal);

        // Act
        var result = await _mockRepository.Object.UpdateGoalProgressAsync(goalId, targetAmount);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsCompleted);
        Assert.NotNull(result.CompletedDate);
        Assert.Equal(100m, result.ProgressPercentage);
        _mockRepository.Verify(r => r.UpdateGoalProgressAsync(goalId, targetAmount, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalStatisticsAsync_ShouldReturnComprehensiveStatistics()
    {
        // Arrange
        var statistics = new GoalStatistics
        {
            TotalGoals = 10,
            ActiveGoals = 6,
            CompletedGoals = 3,
            OverdueGoals = 1,
            TotalTargetAmount = 50000m,
            TotalCurrentAmount = 25000m,
            TotalRemainingAmount = 25000m,
            OverallProgressPercentage = 50m,
            RecentMilestones = 5
        };

        _mockRepository.Setup(r => r.GetGoalStatisticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(statistics);

        // Act
        var result = await _mockRepository.Object.GetGoalStatisticsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.TotalGoals);
        Assert.Equal(6, result.ActiveGoals);
        Assert.Equal(3, result.CompletedGoals);
        Assert.Equal(1, result.OverdueGoals);
        Assert.Equal(50000m, result.TotalTargetAmount);
        Assert.Equal(25000m, result.TotalCurrentAmount);
        Assert.Equal(25000m, result.TotalRemainingAmount);
        Assert.Equal(50m, result.OverallProgressPercentage);
        Assert.Equal(5, result.RecentMilestones);
        _mockRepository.Verify(r => r.GetGoalStatisticsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalStatisticsAsync_WithNoGoals_ShouldReturnZeroStatistics()
    {
        // Arrange
        var emptyStatistics = new GoalStatistics
        {
            TotalGoals = 0,
            ActiveGoals = 0,
            CompletedGoals = 0,
            OverdueGoals = 0,
            TotalTargetAmount = 0m,
            TotalCurrentAmount = 0m,
            TotalRemainingAmount = 0m,
            OverallProgressPercentage = 0m,
            RecentMilestones = 0
        };

        _mockRepository.Setup(r => r.GetGoalStatisticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyStatistics);

        // Act
        var result = await _mockRepository.Object.GetGoalStatisticsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalGoals);
        Assert.Equal(0, result.ActiveGoals);
        Assert.Equal(0, result.CompletedGoals);
        Assert.Equal(0, result.OverdueGoals);
        Assert.Equal(0m, result.TotalTargetAmount);
        Assert.Equal(0m, result.TotalCurrentAmount);
        Assert.Equal(0m, result.TotalRemainingAmount);
        Assert.Equal(0m, result.OverallProgressPercentage);
        Assert.Equal(0, result.RecentMilestones);
        _mockRepository.Verify(r => r.GetGoalStatisticsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Repository_ShouldSupportCancellationToken()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockRepository.Setup(r => r.GetActiveGoalsAsync(cancellationToken))
            .ReturnsAsync(new List<Goal>());

        // Act
        var result = await _mockRepository.Object.GetActiveGoalsAsync(cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.GetActiveGoalsAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Repository_WithCancelledToken_ShouldThrowOperationCancelledException()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;

        _mockRepository.Setup(r => r.GetGoalStatisticsAsync(cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _mockRepository.Object.GetGoalStatisticsAsync(cancellationToken));
    }
}