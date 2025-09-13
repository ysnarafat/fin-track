using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Repository interface for Goal-specific operations
/// </summary>
public interface IGoalRepository : IRepository<Goal>
{
    /// <summary>
    /// Gets goals by type
    /// </summary>
    /// <param name="goalType">Goal type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of goals of the specified type</returns>
    Task<IEnumerable<Goal>> GetByTypeAsync(GoalType goalType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets active (incomplete) goals
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active goals</returns>
    Task<IEnumerable<Goal>> GetActiveGoalsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets completed goals
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of completed goals</returns>
    Task<IEnumerable<Goal>> GetCompletedGoalsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets goals by priority level
    /// </summary>
    /// <param name="priority">Priority level (1-5)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of goals with the specified priority</returns>
    Task<IEnumerable<Goal>> GetByPriorityAsync(int priority, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets goals linked to a specific account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of goals linked to the account</returns>
    Task<IEnumerable<Goal>> GetByLinkedAccountAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets goals with target dates within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of goals with target dates in the range</returns>
    Task<IEnumerable<Goal>> GetByTargetDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets overdue goals (target date passed but not completed)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of overdue goals</returns>
    Task<IEnumerable<Goal>> GetOverdueGoalsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets goals due soon (within specified days)
    /// </summary>
    /// <param name="daysAhead">Number of days to look ahead</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of goals due soon</returns>
    Task<IEnumerable<Goal>> GetGoalsDueSoonAsync(int daysAhead = 30, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates goal progress
    /// </summary>
    /// <param name="goalId">Goal ID</param>
    /// <param name="currentAmount">New current amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateProgressAsync(int goalId, decimal currentAmount, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks a goal as completed
    /// </summary>
    /// <param name="goalId">Goal ID</param>
    /// <param name="completedDate">Completion date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if marked as completed successfully</returns>
    Task<bool> MarkAsCompletedAsync(int goalId, DateTime? completedDate = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets goal progress statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Goal progress statistics</returns>
    Task<GoalProgressStats> GetProgressStatsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets goals with their milestones
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of goals with milestone information</returns>
    Task<IEnumerable<Goal>> GetWithMilestonesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets goals ordered by priority and target date
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of goals ordered by priority and target date</returns>
    Task<IEnumerable<Goal>> GetOrderedByPriorityAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches goals by name or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching goals</returns>
    Task<IEnumerable<Goal>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets goal achievement summary for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Goal achievement summary</returns>
    Task<GoalAchievementSummary> GetAchievementSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Recalculates progress for goals linked to accounts based on account balances
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of goals updated</returns>
    Task<int> RecalculateLinkedAccountProgressAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a goal by name
    /// </summary>
    /// <param name="name">Goal name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Goal if found, null otherwise</returns>
    Task<Goal?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// Goal progress statistics
/// </summary>
public class GoalProgressStats
{
    public int TotalGoals { get; set; }
    public int ActiveGoals { get; set; }
    public int CompletedGoals { get; set; }
    public int OverdueGoals { get; set; }
    public decimal TotalTargetAmount { get; set; }
    public decimal TotalCurrentAmount { get; set; }
    public decimal AverageProgress { get; set; }
    public decimal CompletionRate { get; set; }
}

/// <summary>
/// Goal achievement summary
/// </summary>
public class GoalAchievementSummary
{
    public int GoalsCompleted { get; set; }
    public decimal TotalAmountAchieved { get; set; }
    public IEnumerable<Goal> CompletedGoals { get; set; } = new List<Goal>();
    public decimal AverageTimeToCompletion { get; set; } // in days
    public GoalType MostAchievedGoalType { get; set; }
}