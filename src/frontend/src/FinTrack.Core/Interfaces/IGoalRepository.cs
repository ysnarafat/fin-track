using FinTrack.Core.Entities;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Repository interface for Goal entities with specialized operations
/// </summary>
public interface IGoalRepository : IRepository<Goal>
{
    /// <summary>
    /// Gets goals ordered by priority (highest priority first)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Goals ordered by priority</returns>
    Task<IEnumerable<Goal>> GetGoalsByPriorityAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets active goals (not completed and not overdue)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active goals</returns>
    Task<IEnumerable<Goal>> GetActiveGoalsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets completed goals
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completed goals</returns>
    Task<IEnumerable<Goal>> GetCompletedGoalsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets overdue goals
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Overdue goals</returns>
    Task<IEnumerable<Goal>> GetOverdueGoalsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets goals by category
    /// </summary>
    /// <param name="category">Goal category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Goals in the specified category</returns>
    Task<IEnumerable<Goal>> GetGoalsByCategoryAsync(string category, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets goals with milestones
    /// </summary>
    /// <param name="goalId">Goal ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Goal with its milestones</returns>
    Task<Goal?> GetGoalWithMilestonesAsync(int goalId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets goals that have achieved milestones recently
    /// </summary>
    /// <param name="days">Number of days to look back</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Goals with recent milestone achievements</returns>
    Task<IEnumerable<Goal>> GetGoalsWithRecentMilestonesAsync(int days = 7, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates goal progress and checks for milestone achievements
    /// </summary>
    /// <param name="goalId">Goal ID</param>
    /// <param name="newAmount">New current amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated goal with milestone status</returns>
    Task<Goal?> UpdateGoalProgressAsync(int goalId, decimal newAmount, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets goal statistics summary
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Goal statistics</returns>
    Task<GoalStatistics> GetGoalStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics summary for goals
/// </summary>
public class GoalStatistics
{
    public int TotalGoals { get; set; }
    public int ActiveGoals { get; set; }
    public int CompletedGoals { get; set; }
    public int OverdueGoals { get; set; }
    public decimal TotalTargetAmount { get; set; }
    public decimal TotalCurrentAmount { get; set; }
    public decimal TotalRemainingAmount { get; set; }
    public decimal OverallProgressPercentage { get; set; }
    public int RecentMilestones { get; set; }
}