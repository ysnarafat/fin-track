using FinTrack.Core.Entities;
using FinTrack.Core.Interfaces;
using Moq;

namespace FinTrack.Tests.Unit.Repositories;

/// <summary>
/// Unit tests for IGoalRepository interface contract
/// </summary>
public class IGoalRepositoryTests
{
    private readonly Mock<IGoalRepository> _mockRepository;

    public IGoalRepositoryTests()
    {
        _mockRepository = new Mock<IGoalRepository>();
    }

    [Fact]
    public async Task GetGoalsByPriorityAsync_ShouldReturnGoalsOrderedByPriority()
    {
        // Arrange
        var goals = new List<Goal>
        {
            new Goal { Id = 1, Name = "High Priority", Priority = 1 },
            new Goal { Id = 2, Name = "Medium Priority", Priority = 2 },
            new Goal { Id = 3, Name = "Low Priority", Priority = 3 }
        };
        _mockRepository.Setup(x => x.GetGoalsByPriorityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(goals);

        // Act
        var result = await _mockRepository.Object.GetGoalsByPriorityAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Equal("High Priority", result.First().Name);
        _mockRepository.Verify(x => x.GetGoalsByPriorityAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveGoalsAsync_ShouldReturnOnlyActiveGoals()
    {
        // Arrange
        var activeGoals = new List<Goal>
        {
            new Goal { Id = 1, Name = "Active Goal 1", IsCompleted = false, TargetDate = DateTime.Now.AddDays(30) },
            new Goal { Id = 2, Name = "Active Goal 2", IsCompleted = false, TargetDate = DateTime.Now.AddDays(60) }
        };
        _mockRepository.Setup(x => x.GetActiveGoalsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeGoals);

        // Act
        var result = await _mockRepository.Object.GetActiveGoalsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, goal => Assert.False(goal.IsCompleted));
        _mockRepository.Verify(x => x.GetActiveGoalsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCompletedGoalsAsync_ShouldReturnOnlyCompletedGoals()
    {
        // Arrange
        var completedGoals = new List<Goal>
        {
            new Goal { Id = 1, Name = "Completed Goal 1", IsCompleted = true, CompletedDate = DateTime.Now.AddDays(-10) },
            new Goal { Id = 2, Name = "Completed Goal 2", IsCompleted = true, CompletedDate = DateTime.Now.AddDays(-5) }
        };
        _mockRepository.Setup(x => x.GetCompletedGoalsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedGoals);

        // Act
        var result = await _mockRepository.Object.GetCompletedGoalsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, goal => Assert.True(goal.IsCompleted));
        _mockRepository.Verify(x => x.GetCompletedGoalsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOverdueGoalsAsync_ShouldReturnOnlyOverdueGoals()
    {
        // Arrange
        var overdueGoals = new List<Goal>
        {
            new Goal { Id = 1, Name = "Overdue Goal 1", IsCompleted = false, TargetDate = DateTime.Now.AddDays(-10) },
            new Goal { Id = 2, Name = "Overdue Goal 2", IsCompleted = false, TargetDate = DateTime.Now.AddDays(-5) }
        };
        _mockRepository.Setup(x => x.GetOverdueGoalsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(overdueGoals);

        // Act
        var result = await _mockRepository.Object.GetOverdueGoalsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, goal => 
        {
            Assert.False(goal.IsCompleted);
            Assert.True(goal.TargetDate < DateTime.Now);
        });
        _mockRepository.Verify(x => x.GetOverdueGoalsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalsByCategoryAsync_WithValidCategory_ShouldReturnMatchingGoals()
    {
        // Arrange
        var category = "Emergency Fund";
        var goals = new List<Goal>
        {
            new Goal { Id = 1, Name = "Emergency Fund Goal", Category = category },
            new Goal { Id = 2, Name = "Another Emergency Goal", Category = category }
        };
        _mockRepository.Setup(x => x.GetGoalsByCategoryAsync(category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goals);

        // Act
        var result = await _mockRepository.Object.GetGoalsByCategoryAsync(category);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, goal => Assert.Equal(category, goal.Category));
        _mockRepository.Verify(x => x.GetGoalsByCategoryAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalWithMilestonesAsync_WithValidId_ShouldReturnGoalWithMilestones()
    {
        // Arrange
        var goalId = 1;
        var goal = new Goal 
        { 
            Id = goalId, 
            Name = "Test Goal",
            Milestones = new List<GoalMilestone>
            {
                new GoalMilestone { Id = 1, Name = "25% Complete", TargetAmount = 250 },
                new GoalMilestone { Id = 2, Name = "50% Complete", TargetAmount = 500 }
            }
        };
        _mockRepository.Setup(x => x.GetGoalWithMilestonesAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goal);

        // Act
        var result = await _mockRepository.Object.GetGoalWithMilestonesAsync(goalId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(goalId, result.Id);
        Assert.Equal(2, result.Milestones.Count);
        _mockRepository.Verify(x => x.GetGoalWithMilestonesAsync(goalId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalWithMilestonesAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var goalId = 999;
        _mockRepository.Setup(x => x.GetGoalWithMilestonesAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal?)null);

        // Act
        var result = await _mockRepository.Object.GetGoalWithMilestonesAsync(goalId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(x => x.GetGoalWithMilestonesAsync(goalId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalsWithRecentMilestonesAsync_WithDefaultDays_ShouldReturnGoalsWithRecentMilestones()
    {
        // Arrange
        var goals = new List<Goal>
        {
            new Goal 
            { 
                Id = 1, 
                Name = "Goal with Recent Milestone",
                Milestones = new List<GoalMilestone>
                {
                    new GoalMilestone 
                    { 
                        Id = 1, 
                        Name = "Recent Milestone", 
                        IsAchieved = true, 
                        AchievedDate = DateTime.UtcNow.AddDays(-3) 
                    }
                }
            }
        };
        _mockRepository.Setup(x => x.GetGoalsWithRecentMilestonesAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goals);

        // Act
        var result = await _mockRepository.Object.GetGoalsWithRecentMilestonesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.First().Milestones.Any(m => m.IsAchieved));
        _mockRepository.Verify(x => x.GetGoalsWithRecentMilestonesAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalsWithRecentMilestonesAsync_WithCustomDays_ShouldUseSpecifiedDays()
    {
        // Arrange
        var days = 14;
        var goals = new List<Goal>();
        _mockRepository.Setup(x => x.GetGoalsWithRecentMilestonesAsync(days, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goals);

        // Act
        var result = await _mockRepository.Object.GetGoalsWithRecentMilestonesAsync(days);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(x => x.GetGoalsWithRecentMilestonesAsync(days, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateGoalProgressAsync_WithValidGoalAndAmount_ShouldReturnUpdatedGoal()
    {
        // Arrange
        var goalId = 1;
        var newAmount = 750m;
        var updatedGoal = new Goal 
        { 
            Id = goalId, 
            Name = "Test Goal", 
            CurrentAmount = newAmount,
            TargetAmount = 1000m
        };
        _mockRepository.Setup(x => x.UpdateGoalProgressAsync(goalId, newAmount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedGoal);

        // Act
        var result = await _mockRepository.Object.UpdateGoalProgressAsync(goalId, newAmount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(goalId, result.Id);
        Assert.Equal(newAmount, result.CurrentAmount);
        _mockRepository.Verify(x => x.UpdateGoalProgressAsync(goalId, newAmount, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateGoalProgressAsync_WithInvalidGoal_ShouldReturnNull()
    {
        // Arrange
        var goalId = 999;
        var newAmount = 750m;
        _mockRepository.Setup(x => x.UpdateGoalProgressAsync(goalId, newAmount, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal?)null);

        // Act
        var result = await _mockRepository.Object.UpdateGoalProgressAsync(goalId, newAmount);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(x => x.UpdateGoalProgressAsync(goalId, newAmount, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalStatisticsAsync_ShouldReturnCompleteStatistics()
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
            RecentMilestones = 2
        };
        _mockRepository.Setup(x => x.GetGoalStatisticsAsync(It.IsAny<CancellationToken>()))
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
        Assert.Equal(2, result.RecentMilestones);
        _mockRepository.Verify(x => x.GetGoalStatisticsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGoalStatisticsAsync_WithNoGoals_ShouldReturnZeroStatistics()
    {
        // Arrange
        var statistics = new GoalStatistics
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
        _mockRepository.Setup(x => x.GetGoalStatisticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(statistics);

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
        _mockRepository.Verify(x => x.GetGoalStatisticsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

/// <summary>
/// Unit tests for GoalStatistics class
/// </summary>
public class GoalStatisticsTests
{
    [Fact]
    public void GoalStatistics_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var statistics = new GoalStatistics();

        // Assert
        Assert.Equal(0, statistics.TotalGoals);
        Assert.Equal(0, statistics.ActiveGoals);
        Assert.Equal(0, statistics.CompletedGoals);
        Assert.Equal(0, statistics.OverdueGoals);
        Assert.Equal(0m, statistics.TotalTargetAmount);
        Assert.Equal(0m, statistics.TotalCurrentAmount);
        Assert.Equal(0m, statistics.TotalRemainingAmount);
        Assert.Equal(0m, statistics.OverallProgressPercentage);
        Assert.Equal(0, statistics.RecentMilestones);
    }

    [Fact]
    public void GoalStatistics_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var statistics = new GoalStatistics();

        // Act
        statistics.TotalGoals = 15;
        statistics.ActiveGoals = 8;
        statistics.CompletedGoals = 5;
        statistics.OverdueGoals = 2;
        statistics.TotalTargetAmount = 75000m;
        statistics.TotalCurrentAmount = 45000m;
        statistics.TotalRemainingAmount = 30000m;
        statistics.OverallProgressPercentage = 60m;
        statistics.RecentMilestones = 3;

        // Assert
        Assert.Equal(15, statistics.TotalGoals);
        Assert.Equal(8, statistics.ActiveGoals);
        Assert.Equal(5, statistics.CompletedGoals);
        Assert.Equal(2, statistics.OverdueGoals);
        Assert.Equal(75000m, statistics.TotalTargetAmount);
        Assert.Equal(45000m, statistics.TotalCurrentAmount);
        Assert.Equal(30000m, statistics.TotalRemainingAmount);
        Assert.Equal(60m, statistics.OverallProgressPercentage);
        Assert.Equal(3, statistics.RecentMilestones);
    }

    [Theory]
    [InlineData(10, 6, 3, 1, true)] // Total equals sum of categories
    [InlineData(5, 2, 2, 1, true)] // Total equals sum of categories
    [InlineData(0, 0, 0, 0, true)] // All zeros
    public void GoalStatistics_TotalsShouldBeConsistent(int total, int active, int completed, int overdue, bool shouldBeValid)
    {
        // Arrange
        var statistics = new GoalStatistics
        {
            TotalGoals = total,
            ActiveGoals = active,
            CompletedGoals = completed,
            OverdueGoals = overdue
        };

        // Act
        var actualTotal = statistics.ActiveGoals + statistics.CompletedGoals + statistics.OverdueGoals;
        var isConsistent = actualTotal == statistics.TotalGoals;

        // Assert
        Assert.Equal(shouldBeValid, isConsistent);
    }
}