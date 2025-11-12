using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Interfaces;
using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Goal-specific operations
/// </summary>
public class GoalRepository : BaseRepository<Goal>, IGoalRepository
{
    /// <summary>
    /// Constructor for GoalRepository
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    public GoalRepository(FinTrackDbContext context, ILogger<GoalRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Gets goals by type
    /// </summary>
    public async Task<IEnumerable<Goal>> GetByTypeAsync(GoalType goalType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting goals of type {GoalType}", goalType);
            
            var goals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => g.Type == goalType && !g.IsDeleted)
                .OrderBy(g => g.TargetDate)
                .ThenBy(g => g.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} goals of type {GoalType}", goals.Count, goalType);
            return goals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goals of type {GoalType}", goalType);
            throw;
        }
    }

    /// <summary>
    /// Gets active (incomplete) goals
    /// </summary>
    public async Task<IEnumerable<Goal>> GetActiveGoalsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting active goals");
            
            var goals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => !g.IsCompleted && !g.IsDeleted)
                .OrderBy(g => g.TargetDate)
                .ThenBy(g => g.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} active goals", goals.Count);
            return goals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active goals");
            throw;
        }
    }

    /// <summary>
    /// Gets completed goals
    /// </summary>
    public async Task<IEnumerable<Goal>> GetCompletedGoalsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting completed goals");
            
            var goals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => g.IsCompleted && !g.IsDeleted)
                .OrderByDescending(g => g.CompletedDate)
                .ThenBy(g => g.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} completed goals", goals.Count);
            return goals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed goals");
            throw;
        }
    }

    /// <summary>
    /// Gets goals by priority level
    /// </summary>
    public async Task<IEnumerable<Goal>> GetByPriorityAsync(int priority, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting goals with priority {Priority}", priority);
            
            var goals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => g.Priority == priority && !g.IsDeleted)
                .OrderBy(g => g.TargetDate)
                .ThenBy(g => g.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} goals with priority {Priority}", goals.Count, priority);
            return goals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goals with priority {Priority}", priority);
            throw;
        }
    }

    /// <summary>
    /// Gets goals linked to a specific account
    /// </summary>
    public async Task<IEnumerable<Goal>> GetByLinkedAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting goals linked to account {AccountId}", accountId);
            
            var goals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => g.LinkedAccountId == accountId && !g.IsDeleted)
                .OrderBy(g => g.TargetDate)
                .ThenBy(g => g.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} goals linked to account {AccountId}", goals.Count, accountId);
            return goals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goals linked to account {AccountId}", accountId);
            throw;
        }
    }

    /// <summary>
    /// Gets goals with target dates within a date range
    /// </summary>
    public async Task<IEnumerable<Goal>> GetByTargetDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting goals with target dates from {StartDate} to {EndDate}", startDate, endDate);
            
            var goals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => g.TargetDate >= startDate && g.TargetDate <= endDate && !g.IsDeleted)
                .OrderBy(g => g.TargetDate)
                .ThenBy(g => g.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} goals with target dates from {StartDate} to {EndDate}", 
                goals.Count, startDate, endDate);
            return goals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goals with target dates from {StartDate} to {EndDate}", 
                startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Gets overdue goals (target date passed but not completed)
    /// </summary>
    public async Task<IEnumerable<Goal>> GetOverdueGoalsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var currentDate = DateTime.Today;
            _logger.LogDebug("Getting overdue goals as of {CurrentDate}", currentDate);
            
            var goals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => !g.IsCompleted && g.TargetDate < currentDate && !g.IsDeleted)
                .OrderBy(g => g.TargetDate)
                .ThenBy(g => g.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} overdue goals", goals.Count);
            return goals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue goals");
            throw;
        }
    }

    /// <summary>
    /// Gets goals due soon (within specified days)
    /// </summary>
    public async Task<IEnumerable<Goal>> GetGoalsDueSoonAsync(int daysAhead = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentDate = DateTime.Today;
            var dueDate = currentDate.AddDays(daysAhead);
            
            _logger.LogDebug("Getting goals due within {DaysAhead} days (by {DueDate})", daysAhead, dueDate);
            
            var goals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => !g.IsCompleted && 
                           g.TargetDate >= currentDate && 
                           g.TargetDate <= dueDate && 
                           !g.IsDeleted)
                .OrderBy(g => g.TargetDate)
                .ThenBy(g => g.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} goals due within {DaysAhead} days", goals.Count, daysAhead);
            return goals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goals due within {DaysAhead} days", daysAhead);
            throw;
        }
    }

    /// <summary>
    /// Updates goal progress
    /// </summary>
    public async Task<bool> UpdateProgressAsync(int goalId, decimal currentAmount, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating progress for goal {GoalId} to {CurrentAmount}", goalId, currentAmount);
            
            var goal = await _dbSet
                .Where(g => g.Id == goalId && !g.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (goal == null)
            {
                _logger.LogWarning("Goal {GoalId} not found for progress update", goalId);
                return false;
            }
            
            goal.CurrentAmount = currentAmount;
            
            // Check if goal is now completed
            if (!goal.IsCompleted && goal.CurrentAmount >= goal.TargetAmount)
            {
                goal.IsCompleted = true;
                goal.CompletedDate = DateTime.UtcNow;
            }
            
            goal.MarkAsModified();
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Updated progress for goal {GoalId} to {CurrentAmount}", goalId, currentAmount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for goal {GoalId}", goalId);
            throw;
        }
    }

    /// <summary>
    /// Marks a goal as completed
    /// </summary>
    public async Task<bool> MarkAsCompletedAsync(int goalId, DateTime? completedDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var completionDate = completedDate ?? DateTime.UtcNow;
            _logger.LogDebug("Marking goal {GoalId} as completed on {CompletedDate}", goalId, completionDate);
            
            var goal = await _dbSet
                .Where(g => g.Id == goalId && !g.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (goal == null)
            {
                _logger.LogWarning("Goal {GoalId} not found for completion", goalId);
                return false;
            }
            
            goal.IsCompleted = true;
            goal.CompletedDate = completionDate;
            goal.CurrentAmount = goal.TargetAmount; // Set to target amount if not already there
            goal.MarkAsModified();
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Marked goal {GoalId} as completed", goalId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking goal {GoalId} as completed", goalId);
            throw;
        }
    }

    /// <summary>
    /// Gets goal progress statistics
    /// </summary>
    public async Task<GoalProgressStats> GetProgressStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting goal progress statistics");
            
            var goals = await _dbSet
                .Where(g => !g.IsDeleted)
                .ToListAsync(cancellationToken);
            
            var activeGoals = goals.Where(g => !g.IsCompleted).ToList();
            var completedGoals = goals.Where(g => g.IsCompleted).ToList();
            var overdueGoals = activeGoals.Where(g => g.TargetDate < DateTime.Today).ToList();
            
            var stats = new GoalProgressStats
            {
                TotalGoals = goals.Count,
                ActiveGoals = activeGoals.Count,
                CompletedGoals = completedGoals.Count,
                OverdueGoals = overdueGoals.Count,
                TotalTargetAmount = goals.Sum(g => g.TargetAmount),
                TotalCurrentAmount = goals.Sum(g => g.CurrentAmount),
                AverageProgress = goals.Any() ? goals.Average(g => g.TargetAmount > 0 ? (g.CurrentAmount / g.TargetAmount) * 100 : 0) : 0,
                CompletionRate = goals.Any() ? (completedGoals.Count / (decimal)goals.Count) * 100 : 0
            };
            
            _logger.LogDebug("Retrieved goal progress statistics");
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goal progress statistics");
            throw;
        }
    }

    /// <summary>
    /// Gets goals with their milestones
    /// </summary>
    public async Task<IEnumerable<Goal>> GetWithMilestonesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting goals with milestones");
            
            var goals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => !g.IsDeleted)
                .OrderBy(g => g.TargetDate)
                .ThenBy(g => g.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} goals with milestones", goals.Count);
            return goals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goals with milestones");
            throw;
        }
    }

    /// <summary>
    /// Gets goals ordered by priority and target date
    /// </summary>
    public async Task<IEnumerable<Goal>> GetOrderedByPriorityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting goals ordered by priority and target date");
            
            var goals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => !g.IsDeleted)
                .OrderByDescending(g => g.Priority)
                .ThenBy(g => g.TargetDate)
                .ThenBy(g => g.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} goals ordered by priority", goals.Count);
            return goals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goals ordered by priority");
            throw;
        }
    }

    /// <summary>
    /// Searches goals by name or description
    /// </summary>
    public async Task<IEnumerable<Goal>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Goal>();
                
            _logger.LogDebug("Searching goals by term: {SearchTerm}", searchTerm);
            
            var goals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => (g.Name.Contains(searchTerm) || 
                            (g.Description != null && g.Description.Contains(searchTerm))) && 
                           !g.IsDeleted)
                .OrderBy(g => g.TargetDate)
                .ThenBy(g => g.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Found {Count} goals matching search term: {SearchTerm}", goals.Count, searchTerm);
            return goals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching goals by term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Gets goal achievement summary for a date range
    /// </summary>
    public async Task<GoalAchievementSummary> GetAchievementSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting goal achievement summary from {StartDate} to {EndDate}", startDate, endDate);
            
            var completedGoals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => g.IsCompleted && 
                           g.CompletedDate.HasValue && 
                           g.CompletedDate.Value >= startDate && 
                           g.CompletedDate.Value <= endDate && 
                           !g.IsDeleted)
                .ToListAsync(cancellationToken);
            
            var summary = new GoalAchievementSummary
            {
                GoalsCompleted = completedGoals.Count,
                TotalAmountAchieved = completedGoals.Sum(g => g.TargetAmount),
                CompletedGoals = completedGoals,
                AverageTimeToCompletion = completedGoals.Any() ? 
                    (decimal)completedGoals.Where(g => g.CompletedDate.HasValue)
                                          .Average(g => (g.CompletedDate!.Value - g.CreatedAt).TotalDays) : 0,
                MostAchievedGoalType = completedGoals.Any() ? 
                    completedGoals.GroupBy(g => g.Type)
                                 .OrderByDescending(g => g.Count())
                                 .First().Key : GoalType.Savings
            };
            
            _logger.LogDebug("Retrieved goal achievement summary: {GoalsCompleted} goals completed", 
                summary.GoalsCompleted);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goal achievement summary from {StartDate} to {EndDate}", 
                startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Recalculates progress for goals linked to accounts based on account balances
    /// </summary>
    public async Task<int> RecalculateLinkedAccountProgressAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Recalculating progress for goals linked to accounts");
            
            var linkedGoals = await _dbSet
                .Include(g => g.LinkedAccount)
                .Where(g => g.LinkedAccountId.HasValue && !g.IsCompleted && !g.IsDeleted)
                .ToListAsync(cancellationToken);
            
            var updatedCount = 0;
            
            foreach (var goal in linkedGoals)
            {
                if (goal.LinkedAccount != null)
                {
                    var newProgress = goal.Type switch
                    {
                        GoalType.Savings => goal.LinkedAccount.Balance,
                        GoalType.DebtPayoff => Math.Max(0, goal.TargetAmount - Math.Abs(goal.LinkedAccount.Balance)),
                        _ => goal.CurrentAmount // Keep current amount for other types
                    };
                    
                    if (goal.CurrentAmount != newProgress)
                    {
                        goal.CurrentAmount = newProgress;
                        
                        // Check if goal is now completed
                        if (goal.CurrentAmount >= goal.TargetAmount)
                        {
                            goal.IsCompleted = true;
                            goal.CompletedDate = DateTime.UtcNow;
                        }
                        
                        goal.MarkAsModified();
                        updatedCount++;
                    }
                }
            }
            
            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            
            _logger.LogDebug("Recalculated progress for {Count} linked account goals", updatedCount);
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating progress for linked account goals");
            throw;
        }
    }

    /// <summary>
    /// Gets a goal by name
    /// </summary>
    public async Task<Goal?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting goal by name: {Name}", name);
            
            var goal = await _dbSet
                .Where(g => !g.IsDeleted && g.Name == name)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (goal == null)
            {
                _logger.LogDebug("Goal with name {Name} not found", name);
            }
            
            return goal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting goal by name: {Name}", name);
            throw;
        }
    }
}