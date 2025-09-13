using FinTrack.Maui.Models;

namespace FinTrack.Maui.Services;

/// <summary>
/// Service interface for goal management operations
/// </summary>
public interface IGoalService
{
    /// <summary>
    /// Gets all goals
    /// </summary>
    Task<IEnumerable<Goal>> GetGoalsAsync();
    
    /// <summary>
    /// Gets goals ordered by priority (highest priority first)
    /// </summary>
    Task<IEnumerable<Goal>> GetGoalsByPriorityAsync();
    
    /// <summary>
    /// Gets active goals (not completed and not overdue)
    /// </summary>
    Task<IEnumerable<Goal>> GetActiveGoalsAsync();
    
    /// <summary>
    /// Gets completed goals
    /// </summary>
    Task<IEnumerable<Goal>> GetCompletedGoalsAsync();
    
    /// <summary>
    /// Gets overdue goals
    /// </summary>
    Task<IEnumerable<Goal>> GetOverdueGoalsAsync();
    
    /// <summary>
    /// Gets a specific goal by ID
    /// </summary>
    Task<Goal?> GetGoalAsync(int id);
    
    /// <summary>
    /// Gets a goal with its milestones
    /// </summary>
    Task<Goal?> GetGoalWithMilestonesAsync(int id);
    
    /// <summary>
    /// Creates a new goal
    /// </summary>
    Task<Goal?> CreateGoalAsync(Goal goal);
    
    /// <summary>
    /// Updates an existing goal
    /// </summary>
    Task<bool> UpdateGoalAsync(Goal goal);
    
    /// <summary>
    /// Updates goal progress and checks for milestone achievements
    /// </summary>
    Task<bool> UpdateGoalProgressAsync(int goalId, decimal newAmount);
    
    /// <summary>
    /// Deletes a goal
    /// </summary>
    Task<bool> DeleteGoalAsync(int id);
    
    /// <summary>
    /// Gets goal statistics summary
    /// </summary>
    Task<GoalStatistics> GetGoalStatisticsAsync();
    
    /// <summary>
    /// Gets recent milestone achievements (last 7 days)
    /// </summary>
    Task<IEnumerable<GoalMilestone>> GetRecentAchievementsAsync();
    
    /// <summary>
    /// Gets goals by category
    /// </summary>
    Task<IEnumerable<Goal>> GetGoalsByCategoryAsync(string category);
    
    /// <summary>
    /// Adds a milestone to a goal
    /// </summary>
    Task<bool> AddMilestoneAsync(int goalId, GoalMilestone milestone);
    
    /// <summary>
    /// Updates a milestone
    /// </summary>
    Task<bool> UpdateMilestoneAsync(GoalMilestone milestone);
    
    /// <summary>
    /// Deletes a milestone
    /// </summary>
    Task<bool> DeleteMilestoneAsync(int milestoneId);
    
    /// <summary>
    /// Gets available goal categories
    /// </summary>
    Task<IEnumerable<string>> GetGoalCategoriesAsync();
}