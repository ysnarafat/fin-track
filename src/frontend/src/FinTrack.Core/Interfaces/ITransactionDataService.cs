using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Data service interface for Transaction-specific business operations
/// </summary>
public interface ITransactionDataService : IDataService<Transaction>
{
    /// <summary>
    /// Creates a new transaction and updates account balance
    /// </summary>
    /// <param name="transaction">Transaction to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transaction</returns>
    Task<Transaction> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a transaction and adjusts account balances
    /// </summary>
    /// <param name="transaction">Transaction to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated transaction</returns>
    Task<Transaction> UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a transaction and adjusts account balance
    /// </summary>
    /// <param name="transactionId">Transaction ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteTransactionAsync(int transactionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a transfer transaction between two accounts
    /// </summary>
    /// <param name="fromAccountId">Source account ID</param>
    /// <param name="toAccountId">Destination account ID</param>
    /// <param name="amount">Transfer amount</param>
    /// <param name="description">Transfer description</param>
    /// <param name="date">Transfer date</param>
    /// <param name="categoryId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of both transfer transactions (from, to)</returns>
    Task<(Transaction FromTransaction, Transaction ToTransaction)> CreateTransferAsync(
        int fromAccountId, int toAccountId, decimal amount, string description, 
        DateTime date, int categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets transactions with full related data (account, category)
    /// </summary>
    /// <param name="accountId">Optional account ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions with related data</returns>
    Task<IEnumerable<Transaction>> GetTransactionsWithDetailsAsync(int? accountId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets recent transactions with related data
    /// </summary>
    /// <param name="count">Number of transactions to retrieve</param>
    /// <param name="accountId">Optional account ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of recent transactions with related data</returns>
    Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int count, int? accountId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets transactions for a specific month with related data
    /// </summary>
    /// <param name="year">Year</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="accountId">Optional account ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of monthly transactions with related data</returns>
    Task<IEnumerable<Transaction>> GetMonthlyTransactionsAsync(int year, int month, int? accountId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches transactions with related data
    /// </summary>
    /// <param name="searchTerm">Search term for description</param>
    /// <param name="accountId">Optional account ID to filter by</param>
    /// <param name="categoryId">Optional category ID to filter by</param>
    /// <param name="transactionType">Optional transaction type to filter by</param>
    /// <param name="startDate">Optional start date to filter by</param>
    /// <param name="endDate">Optional end date to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching transactions with related data</returns>
    Task<IEnumerable<Transaction>> SearchTransactionsAsync(
        string? searchTerm = null,
        int? accountId = null,
        int? categoryId = null,
        TransactionType? transactionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reconciles transactions
    /// </summary>
    /// <param name="transactionIds">Transaction IDs to reconcile</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of transactions reconciled</returns>
    Task<int> ReconcileTransactionsAsync(IEnumerable<int> transactionIds, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets transaction summary for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="accountId">Optional account ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction summary with totals</returns>
    Task<TransactionSummary> GetTransactionSummaryAsync(DateTime startDate, DateTime endDate, int? accountId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets spending analysis by category
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="accountId">Optional account ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of category spending analysis</returns>
    Task<IEnumerable<CategorySpending>> GetSpendingAnalysisAsync(DateTime startDate, DateTime endDate, int? accountId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets monthly spending trends
    /// </summary>
    /// <param name="year">Year</param>
    /// <param name="accountId">Optional account ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of monthly spending data</returns>
    Task<IEnumerable<MonthlySpending>> GetMonthlySpendingTrendsAsync(int year, int? accountId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Duplicates a transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID to duplicate</param>
    /// <param name="newDate">New date for the duplicated transaction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Duplicated transaction</returns>
    Task<Transaction> DuplicateTransactionAsync(int transactionId, DateTime newDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Bulk imports transactions
    /// </summary>
    /// <param name="transactions">Transactions to import</param>
    /// <param name="skipDuplicates">Whether to skip duplicate transactions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result with success/failure counts</returns>
    Task<ImportResult> BulkImportTransactionsAsync(IEnumerable<Transaction> transactions, bool skipDuplicates = true, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a transaction summary for a date range
/// </summary>
public class TransactionSummary
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetAmount { get; set; }
    public int TransactionCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}



/// <summary>
/// Represents monthly spending data
/// </summary>
public class MonthlySpending
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalSpending { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal NetAmount { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// Represents the result of a bulk import operation
/// </summary>
public class ImportResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<Transaction> ImportedTransactions { get; set; } = new();
}