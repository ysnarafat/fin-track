using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Interfaces;
using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Transaction-specific operations
/// </summary>
public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
{
    /// <summary>
    /// Constructor for TransactionRepository
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    public TransactionRepository(FinTrackDbContext context, ILogger<TransactionRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Gets transactions for a specific account
    /// </summary>
    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting transactions for account {AccountId}", accountId);
            
            var transactions = await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted && t.AccountId == accountId)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} transactions for account {AccountId}", transactions.Count, accountId);
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for account {AccountId}", accountId);
            throw;
        }
    }

    /// <summary>
    /// Gets transactions for a specific category
    /// </summary>
    public async Task<IEnumerable<Transaction>> GetByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting transactions for category {CategoryId}", categoryId);
            
            var transactions = await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted && t.CategoryId == categoryId)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} transactions for category {CategoryId}", transactions.Count, categoryId);
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for category {CategoryId}", categoryId);
            throw;
        }
    }

    /// <summary>
    /// Gets transactions within a date range
    /// </summary>
    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting transactions from {StartDate} to {EndDate}", startDate, endDate);
            
            var transactions = await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted && t.Date >= startDate && t.Date <= endDate)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} transactions from {StartDate} to {EndDate}", transactions.Count, startDate, endDate);
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions from {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Gets transactions for a specific account within a date range
    /// </summary>
    public async Task<IEnumerable<Transaction>> GetByAccountAndDateRangeAsync(int accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting transactions for account {AccountId} from {StartDate} to {EndDate}", accountId, startDate, endDate);
            
            var transactions = await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted && t.AccountId == accountId && t.Date >= startDate && t.Date <= endDate)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} transactions for account {AccountId} from {StartDate} to {EndDate}", 
                transactions.Count, accountId, startDate, endDate);
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for account {AccountId} from {StartDate} to {EndDate}", 
                accountId, startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Gets transactions by type
    /// </summary>
    public async Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType transactionType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting transactions of type {TransactionType}", transactionType);
            
            var transactions = await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted && t.Type == transactionType)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} transactions of type {TransactionType}", transactions.Count, transactionType);
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions of type {TransactionType}", transactionType);
            throw;
        }
    }

    /// <summary>
    /// Gets recent transactions (ordered by date descending)
    /// </summary>
    public async Task<IEnumerable<Transaction>> GetRecentAsync(int count, int? accountId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {Count} recent transactions for account {AccountId}", count, accountId?.ToString() ?? "all");
            
            var query = _dbSet
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted);
                
            if (accountId.HasValue)
            {
                query = query.Where(t => t.AccountId == accountId.Value);
            }
            
            var transactions = await query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Take(count)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} recent transactions", transactions.Count);
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent transactions");
            throw;
        }
    }

    /// <summary>
    /// Gets transactions for a specific month and year
    /// </summary>
    public async Task<IEnumerable<Transaction>> GetByMonthAsync(int year, int month, int? accountId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting transactions for {Year}-{Month:D2} for account {AccountId}", year, month, accountId?.ToString() ?? "all");
            
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var query = _dbSet
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted && t.Date >= startDate && t.Date <= endDate);
                
            if (accountId.HasValue)
            {
                query = query.Where(t => t.AccountId == accountId.Value);
            }
            
            var transactions = await query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} transactions for {Year}-{Month:D2}", transactions.Count, year, month);
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for {Year}-{Month:D2}", year, month);
            throw;
        }
    }

    /// <summary>
    /// Searches transactions by description
    /// </summary>
    public async Task<IEnumerable<Transaction>> SearchByDescriptionAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Transaction>();
                
            _logger.LogDebug("Searching transactions by description: {SearchTerm}", searchTerm);
            
            var transactions = await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted && 
                           (t.Description.Contains(searchTerm) || 
                            (t.Notes != null && t.Notes.Contains(searchTerm)) ||
                            (t.ReferenceNumber != null && t.ReferenceNumber.Contains(searchTerm))))
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Found {Count} transactions matching search term", transactions.Count);
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching transactions by description: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Gets unreconciled transactions for an account
    /// </summary>
    public async Task<IEnumerable<Transaction>> GetUnreconciledAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting unreconciled transactions for account {AccountId}", accountId);
            
            var transactions = await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted && t.AccountId == accountId && !t.IsReconciled)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} unreconciled transactions for account {AccountId}", transactions.Count, accountId);
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unreconciled transactions for account {AccountId}", accountId);
            throw;
        }
    }

    /// <summary>
    /// Gets transfer transactions (both sides of the transfer)
    /// </summary>
    public async Task<IEnumerable<Transaction>> GetTransfersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting transfer transactions");
            
            var transactions = await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Include(t => t.TransferToAccount)
                .Where(t => !t.IsDeleted && t.Type == TransactionType.Transfer)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} transfer transactions", transactions.Count);
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transfer transactions");
            throw;
        }
    }

    /// <summary>
    /// Gets linked transaction for a transfer
    /// </summary>
    public async Task<Transaction?> GetLinkedTransactionAsync(int transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting linked transaction for transaction {TransactionId}", transactionId);
            
            var transaction = await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted && t.Id == transactionId)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (transaction?.LinkedTransactionId == null)
            {
                _logger.LogDebug("No linked transaction found for transaction {TransactionId}", transactionId);
                return null;
            }
            
            var linkedTransaction = await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted && t.Id == transaction.LinkedTransactionId.Value)
                .FirstOrDefaultAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved linked transaction {LinkedTransactionId} for transaction {TransactionId}", 
                linkedTransaction?.Id, transactionId);
            return linkedTransaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting linked transaction for transaction {TransactionId}", transactionId);
            throw;
        }
    }

    /// <summary>
    /// Calculates total income for an account within a date range
    /// </summary>
    public async Task<decimal> CalculateIncomeAsync(int? accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Calculating income for account {AccountId} from {StartDate} to {EndDate}", 
                accountId?.ToString() ?? "all", startDate, endDate);
            
            var query = _dbSet
                .Where(t => !t.IsDeleted && 
                           t.Type == TransactionType.Income && 
                           t.Date >= startDate && 
                           t.Date <= endDate);
                           
            if (accountId.HasValue)
            {
                query = query.Where(t => t.AccountId == accountId.Value);
            }
            
            var totalIncome = await query.SumAsync(t => t.Amount, cancellationToken);
            
            _logger.LogDebug("Calculated total income: {TotalIncome}", totalIncome);
            return totalIncome;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating income for account {AccountId}", accountId);
            throw;
        }
    }

    /// <summary>
    /// Calculates total expenses for an account within a date range
    /// </summary>
    public async Task<decimal> CalculateExpensesAsync(int? accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Calculating expenses for account {AccountId} from {StartDate} to {EndDate}", 
                accountId?.ToString() ?? "all", startDate, endDate);
            
            var query = _dbSet
                .Where(t => !t.IsDeleted && 
                           t.Type == TransactionType.Expense && 
                           t.Date >= startDate && 
                           t.Date <= endDate);
                           
            if (accountId.HasValue)
            {
                query = query.Where(t => t.AccountId == accountId.Value);
            }
            
            var totalExpenses = await query.SumAsync(t => t.Amount, cancellationToken);
            
            _logger.LogDebug("Calculated total expenses: {TotalExpenses}", totalExpenses);
            return totalExpenses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating expenses for account {AccountId}", accountId);
            throw;
        }
    }

    /// <summary>
    /// Gets spending by category within a date range
    /// </summary>
    public async Task<Dictionary<int, decimal>> GetSpendingByCategoryAsync(DateTime startDate, DateTime endDate, int? accountId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting spending by category from {StartDate} to {EndDate} for account {AccountId}", 
                startDate, endDate, accountId?.ToString() ?? "all");
            
            var query = _dbSet
                .Where(t => !t.IsDeleted && 
                           t.Type == TransactionType.Expense && 
                           t.Date >= startDate && 
                           t.Date <= endDate);
                           
            if (accountId.HasValue)
            {
                query = query.Where(t => t.AccountId == accountId.Value);
            }
            
            var spendingByCategory = await query
                .GroupBy(t => t.CategoryId)
                .Select(g => new { CategoryId = g.Key, TotalSpending = g.Sum(t => t.Amount) })
                .ToDictionaryAsync(x => x.CategoryId, x => x.TotalSpending, cancellationToken);
                
            _logger.LogDebug("Retrieved spending for {Count} categories", spendingByCategory.Count);
            return spendingByCategory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting spending by category");
            throw;
        }
    }

    /// <summary>
    /// Gets monthly spending totals for a year
    /// </summary>
    public async Task<Dictionary<int, decimal>> GetMonthlySpendingAsync(int year, int? accountId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting monthly spending for year {Year} for account {AccountId}", 
                year, accountId?.ToString() ?? "all");
            
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);
            
            var query = _dbSet
                .Where(t => !t.IsDeleted && 
                           t.Type == TransactionType.Expense && 
                           t.Date >= startDate && 
                           t.Date <= endDate);
                           
            if (accountId.HasValue)
            {
                query = query.Where(t => t.AccountId == accountId.Value);
            }
            
            var monthlySpending = await query
                .GroupBy(t => t.Date.Month)
                .Select(g => new { Month = g.Key, TotalSpending = g.Sum(t => t.Amount) })
                .ToDictionaryAsync(x => x.Month, x => x.TotalSpending, cancellationToken);
                
            _logger.LogDebug("Retrieved monthly spending for {Count} months", monthlySpending.Count);
            return monthlySpending;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly spending for year {Year}", year);
            throw;
        }
    }

    /// <summary>
    /// Bulk reconciles transactions
    /// </summary>
    public async Task<int> BulkReconcileAsync(IEnumerable<int> transactionIds, DateTime reconciledAt, CancellationToken cancellationToken = default)
    {
        try
        {
            var idList = transactionIds.ToList();
            _logger.LogDebug("Bulk reconciling {Count} transactions", idList.Count);
            
            var transactions = await _dbSet
                .Where(t => !t.IsDeleted && idList.Contains(t.Id))
                .ToListAsync(cancellationToken);
                
            foreach (var transaction in transactions)
            {
                transaction.IsReconciled = true;
                transaction.ReconciledAt = reconciledAt;
                transaction.MarkAsModified();
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Bulk reconciled {Count} transactions", transactions.Count);
            return transactions.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk reconciling transactions");
            throw;
        }
    }
}