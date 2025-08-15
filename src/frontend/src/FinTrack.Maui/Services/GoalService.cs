using FinTrack.Maui.Models;

namespace FinTrack.Maui.Services;

/// <summary>
/// Mock implementation of goal service for demonstration purposes
/// In a real application, this would interact with the repository layer
/// </summary>
public class GoalService : IGoalService
{
    private readonly List<Goal> _goals;
    private int _nextId = 1;
    
    public GoalService()
    {
        _goals = GenerateMockGoals();
    }
    
    public async Task<IEnumerable<Goal>> GetGoalsAsync()
    {
        await Task.Delay(100); // Simulate async operation
        return _goals.Where(g => !g.IsCompleted).OrderBy(g => g.Priority).ToList();
    }
    
    public async Task<IEnumerable<Goal>> GetGoalsByPriorityAsync()
    {
        await Task.Delay(100);
        return _goals.OrderBy(g => g.Priority).ThenBy(g => g.TargetDate).ToList();
    }
    
    public async Task<IEnumerable<Goal>> GetActiveGoalsAsync()
    {
        await Task.Delay(100);
        return _goals.Where(g => !g.IsCompleted && !g.IsOverdue).OrderBy(g => g.Priority).ToList();
    }
    
    public async Task<IEnumerable<Goal>> GetCompletedGoalsAsync()
    {
        await Task.Delay(100);
        return _goals.Where(g => g.IsCompleted).OrderByDescending(g => g.CompletedDate).ToList();
    }
    
    public async Task<IEnumerable<Goal>> GetOverdueGoalsAsync()
    {
        await Task.Delay(100);
        return _goals.Where(g => g.IsOverdue).OrderBy(g => g.TargetDate).ToList();
    }
    
    public async Task<Goal?> GetGoalAsync(int id)
    {
        await Task.Delay(50);
        return _goals.FirstOrDefault(g => g.Id == id);
    }
    
    public async Task<Goal?> GetGoalWithMilestonesAsync(int id)
    {
        await Task.Delay(50);
        return _goals.FirstOrDefault(g => g.Id == id);
    }
    
    public async Task<Goal?> CreateGoalAsync(Goal goal)
    {
        await Task.Delay(200);
        
        goal.Id = _nextId++;
        _goals.Add(goal);
        
        return goal;
    }
    
    public async Task<bool> UpdateGoalAsync(Goal goal)
    {
        await Task.Delay(200);
        
        var existingGoal = _goals.FirstOrDefault(g => g.Id == goal.Id);
        if (existingGoal == null) return false;
        
        existingGoal.Name = goal.Name;
        existingGoal.Description = goal.Description;
        existingGoal.TargetAmount = goal.TargetAmount;
        existingGoal.CurrentAmount = goal.CurrentAmount;
        existingGoal.TargetDate = goal.TargetDate;
        existingGoal.Priority = goal.Priority;
        existingGoal.Category = goal.Category;
        existingGoal.Color = goal.Color;
        existingGoal.Milestones = goal.Milestones;
        
        return true;
    }
    
    public async Task<bool> UpdateGoalProgressAsync(int goalId, decimal newAmount)
    {
        await Task.Delay(100);
        
        var goal = _goals.FirstOrDefault(g => g.Id == goalId);
        if (goal == null) return false;
        
        var previousAmount = goal.CurrentAmount;
        goal.CurrentAmount = newAmount;
        
        // Check if goal is completed
        if (newAmount >= goal.TargetAmount && !goal.IsCompleted)
        {
            goal.IsCompleted = true;
            goal.CompletedDate = DateTime.UtcNow;
        }
        else if (newAmount < goal.TargetAmount && goal.IsCompleted)
        {
            goal.IsCompleted = false;
            goal.CompletedDate = null;
        }
        
        // Check for milestone achievements
        foreach (var milestone in goal.Milestones.Where(m => !m.IsAchieved))
        {
            if (newAmount >= milestone.TargetAmount)
            {
                milestone.IsAchieved = true;
                milestone.AchievedDate = DateTime.UtcNow;
            }
        }
        
        return true;
    }
    
    public async Task<bool> DeleteGoalAsync(int id)
    {
        await Task.Delay(100);
        
        var goal = _goals.FirstOrDefault(g => g.Id == id);
        if (goal == null) return false;
        
        _goals.Remove(goal);
        return true;
    }
    
    public async Task<GoalStatistics> GetGoalStatisticsAsync()
    {
        await Task.Delay(100);
        
        var totalGoals = _goals.Count;
        var activeGoals = _goals.Count(g => !g.IsCompleted && !g.IsOverdue);
        var completedGoals = _goals.Count(g => g.IsCompleted);
        var overdueGoals = _goals.Count(g => g.IsOverdue);
        var totalTargetAmount = _goals.Sum(g => g.TargetAmount);
        var totalCurrentAmount = _goals.Sum(g => g.CurrentAmount);
        var recentMilestones = _goals.SelectMany(g => g.Milestones)
            .Count(m => m.IsAchieved && m.AchievedDate.HasValue && m.AchievedDate.Value >= DateTime.Now.AddDays(-7));
        
        return new GoalStatistics
        {
            TotalGoals = totalGoals,
            ActiveGoals = activeGoals,
            CompletedGoals = completedGoals,
            OverdueGoals = overdueGoals,
            TotalTargetAmount = totalTargetAmount,
            TotalCurrentAmount = totalCurrentAmount,
            TotalRemainingAmount = totalTargetAmount - totalCurrentAmount,
            OverallProgressPercentage = totalTargetAmount > 0 ? (double)(totalCurrentAmount / totalTargetAmount * 100) : 0,
            RecentMilestones = recentMilestones
        };
    }
    
    public async Task<IEnumerable<GoalMilestone>> GetRecentAchievementsAsync()
    {
        await Task.Delay(50);
        
        return _goals.SelectMany(g => g.Milestones)
            .Where(m => m.IsAchieved && m.AchievedDate.HasValue && m.AchievedDate.Value >= DateTime.Now.AddDays(-7))
            .OrderByDescending(m => m.AchievedDate)
            .ToList();
    }
    
    public async Task<IEnumerable<Goal>> GetGoalsByCategoryAsync(string category)
    {
        await Task.Delay(50);
        return _goals.Where(g => g.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }
    
    public async Task<bool> AddMilestoneAsync(int goalId, GoalMilestone milestone)
    {
        await Task.Delay(100);
        
        var goal = _goals.FirstOrDefault(g => g.Id == goalId);
        if (goal == null) return false;
        
        milestone.Id = goal.Milestones.Count + 1;
        milestone.GoalId = goalId;
        goal.Milestones.Add(milestone);
        
        return true;
    }
    
    public async Task<bool> UpdateMilestoneAsync(GoalMilestone milestone)
    {
        await Task.Delay(100);
        
        var goal = _goals.FirstOrDefault(g => g.Id == milestone.GoalId);
        var existingMilestone = goal?.Milestones.FirstOrDefault(m => m.Id == milestone.Id);
        
        if (existingMilestone == null) return false;
        
        existingMilestone.Name = milestone.Name;
        existingMilestone.Description = milestone.Description;
        existingMilestone.TargetAmount = milestone.TargetAmount;
        
        return true;
    }
    
    public async Task<bool> DeleteMilestoneAsync(int milestoneId)
    {
        await Task.Delay(100);
        
        foreach (var goal in _goals)
        {
            var milestone = goal.Milestones.FirstOrDefault(m => m.Id == milestoneId);
            if (milestone != null)
            {
                goal.Milestones.Remove(milestone);
                return true;
            }
        }
        
        return false;
    }
    
    public async Task<IEnumerable<string>> GetGoalCategoriesAsync()
    {
        await Task.Delay(50);
        
        return new[]
        {
            "Emergency Fund",
            "Vacation",
            "Car",
            "House",
            "Education",
            "Retirement",
            "Investment",
            "Debt Payoff",
            "Other"
        };
    }
    
    private List<Goal> GenerateMockGoals()
    {
        var goals = new List<Goal>
        {
            new Goal
            {
                Id = _nextId++,
                Name = "Emergency Fund",
                Description = "Build a 6-month emergency fund for financial security",
                TargetAmount = 15000m,
                CurrentAmount = 8500m,
                TargetDate = DateTime.Today.AddMonths(8),
                Priority = 1,
                Category = "Emergency Fund",
                Color = "#EF4444",
                Milestones = new List<GoalMilestone>
                {
                    new GoalMilestone { Id = 1, Name = "First $5,000", TargetAmount = 5000m, IsAchieved = true, AchievedDate = DateTime.Now.AddDays(-30) },
                    new GoalMilestone { Id = 2, Name = "Halfway Point", TargetAmount = 7500m, IsAchieved = true, AchievedDate = DateTime.Now.AddDays(-10) },
                    new GoalMilestone { Id = 3, Name = "Three Quarters", TargetAmount = 11250m, IsAchieved = false }
                }
            },
            new Goal
            {
                Id = _nextId++,
                Name = "Dream Vacation",
                Description = "Save for a 2-week trip to Europe",
                TargetAmount = 5000m,
                CurrentAmount = 1200m,
                TargetDate = DateTime.Today.AddMonths(10),
                Priority = 3,
                Category = "Vacation",
                Color = "#3B82F6",
                Milestones = new List<GoalMilestone>
                {
                    new GoalMilestone { Id = 4, Name = "Flight Money", TargetAmount = 1500m, IsAchieved = false },
                    new GoalMilestone { Id = 5, Name = "Accommodation", TargetAmount = 3000m, IsAchieved = false }
                }
            },
            new Goal
            {
                Id = _nextId++,
                Name = "New Car Down Payment",
                Description = "Save for a 20% down payment on a new car",
                TargetAmount = 8000m,
                CurrentAmount = 3200m,
                TargetDate = DateTime.Today.AddMonths(6),
                Priority = 2,
                Category = "Car",
                Color = "#10B981",
                Milestones = new List<GoalMilestone>
                {
                    new GoalMilestone { Id = 6, Name = "Quarter Way", TargetAmount = 2000m, IsAchieved = true, AchievedDate = DateTime.Now.AddDays(-45) },
                    new GoalMilestone { Id = 7, Name = "Halfway Point", TargetAmount = 4000m, IsAchieved = false }
                }
            },
            new Goal
            {
                Id = _nextId++,
                Name = "Home Renovation",
                Description = "Kitchen and bathroom renovation fund",
                TargetAmount = 25000m,
                CurrentAmount = 12000m,
                TargetDate = DateTime.Today.AddMonths(18),
                Priority = 4,
                Category = "House",
                Color = "#F59E0B",
                Milestones = new List<GoalMilestone>
                {
                    new GoalMilestone { Id = 8, Name = "Kitchen Planning", TargetAmount = 15000m, IsAchieved = false },
                    new GoalMilestone { Id = 9, Name = "Bathroom Budget", TargetAmount = 20000m, IsAchieved = false }
                }
            },
            new Goal
            {
                Id = _nextId++,
                Name = "Investment Portfolio",
                Description = "Build initial investment portfolio",
                TargetAmount = 10000m,
                CurrentAmount = 10000m,
                TargetDate = DateTime.Today.AddDays(-30), // Completed goal
                Priority = 2,
                Category = "Investment",
                Color = "#8B5CF6",
                IsCompleted = true,
                CompletedDate = DateTime.Today.AddDays(-30),
                Milestones = new List<GoalMilestone>
                {
                    new GoalMilestone { Id = 10, Name = "First $2,500", TargetAmount = 2500m, IsAchieved = true, AchievedDate = DateTime.Now.AddDays(-90) },
                    new GoalMilestone { Id = 11, Name = "Halfway Point", TargetAmount = 5000m, IsAchieved = true, AchievedDate = DateTime.Now.AddDays(-60) },
                    new GoalMilestone { Id = 12, Name = "Final Push", TargetAmount = 7500m, IsAchieved = true, AchievedDate = DateTime.Now.AddDays(-45) }
                }
            }
        };
        
        _nextId = goals.Count + 1;
        return goals;
    }
}