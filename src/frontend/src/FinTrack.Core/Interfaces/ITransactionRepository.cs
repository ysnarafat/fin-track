using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Repository interface for Transaction-specific operations
/// </summary>
public interface ITransactionRepository : IRepository<Transaction>
{
    /// <summary>
    /// Gets transactions for a specific account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions for the account</returns>
    Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets transactions for a specific category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions for the category</returns>
    Task<IEnumerable<Transaction>> GetByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets transactions within a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions within the date range</returns>
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets transactions for a specific account within a date range
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions for the account within the date range</returns>
    Task<IEnumerable<Transaction>> GetByAccountAndDateRangeAsync(int accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets transactions by type
    /// </summary>
    /// <param name="transactionType">Transaction type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions of the specified type</returns>
    Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType transactionType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets recent transactions (ordered by date descending)
    /// </summary>
    /// <param name="count">Number of transactions to retrieve</param>
    /// <param name="accountId">Optional account ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of recent transactions</returns>
    Task<IEnumerable<Transaction>> GetRecentAsync(int count, int? accountId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets transactions for a specific month and year
    /// </summary>
    /// <param name="year">Year</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="accountId">Optional account ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions for the specified month</returns>
    Task<IEnumerable<Transaction>> GetByMonthAsync(int year, int month, int? accountId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches transactions by description
    /// </summary>
    /// <param name="searchTerm">Search term to match against description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions matching the search term</returns>
    Task<IEnumerable<Transaction>> SearchByDescriptionAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets unreconciled transactions for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of unreconciled transactions</returns>
    Task<IEnumerable<Transaction>> GetUnreconciledAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets transfer transactions (both sides of the transfer)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transfer transactions</returns>
    Task<IEnumerable<Transaction>> GetTransfersAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets linked transaction for a transfer
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Linked transaction if found, null otherwise</returns>
    Task<Transaction?> GetLinkedTransactionAsync(int transactionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculates total income for an account within a date range
    /// </summary>
    /// <param name="accountId">Account ID (optional, if null calculates for all accounts)</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total income amount</returns>
    Task<decimal> CalculateIncomeAsync(int? accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculates total expenses for an account within a date range
    /// </summary>
    /// <param name="accountId">Account ID (optional, if null calculates for all accounts)</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total expenses amount</returns>
    Task<decimal> CalculateExpensesAsync(int? accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets spending by category within a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="accountId">Optional account ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of category ID to total spending amount</returns>
    Task<Dictionary<int, decimal>> GetSpendingByCategoryAsync(DateTime startDate, DateTime endDate, int? accountId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets monthly spending totals for a year
    /// </summary>
    /// <param name="year">Year</param>
    /// <param name="accountId">Optional account ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of month (1-12) to total spending amount</returns>
    Task<Dictionary<int, decimal>> GetMonthlySpendingAsync(int year, int? accountId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Bulk reconciles transactions
    /// </summary>
    /// <param name="transactionIds">Transaction IDs to reconcile</param>
    /// <param name="reconciledAt">Reconciliation timestamp</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of transactions reconciled</returns>
    Task<int> BulkReconcileAsync(IEnumerable<int> transactionIds, DateTime reconciledAt, CancellationToken cancellationToken = default);
}