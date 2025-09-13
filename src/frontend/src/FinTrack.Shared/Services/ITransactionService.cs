using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Shared.Services;

/// <summary>
/// Service interface for transaction business logic and operations
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Creates a new transaction with business logic validation
    /// </summary>
    /// <param name="transaction">Transaction to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transaction</returns>
    Task<Transaction> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing transaction with business logic validation
    /// </summary>
    /// <param name="transaction">Transaction to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated transaction</returns>
    Task<Transaction> UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a transaction and reverses its effects on account balance
    /// </summary>
    /// <param name="transactionId">Transaction ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteTransactionAsync(int transactionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a transaction by ID
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction if found, null otherwise</returns>
    Task<Transaction?> GetTransactionAsync(int transactionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets transactions for an account with optional filtering
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="transactionType">Optional transaction type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions</returns>
    Task<IEnumerable<Transaction>> GetTransactionsAsync(
        int? accountId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? categoryId = null,
        TransactionType? transactionType = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a transfer transaction between two accounts
    /// </summary>
    /// <param name="fromAccountId">Source account ID</param>
    /// <param name="toAccountId">Destination account ID</param>
    /// <param name="amount">Transfer amount</param>
    /// <param name="description">Transfer description</param>
    /// <param name="date">Transfer date</param>
    /// <param name="categoryId">Category ID for the transfer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of source and destination transactions</returns>
    Task<(Transaction sourceTransaction, Transaction destinationTransaction)> CreateTransferAsync(
        int fromAccountId,
        int toAccountId,
        decimal amount,
        string description,
        DateTime date,
        int categoryId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reconciles transactions by marking them as reconciled
    /// </summary>
    /// <param name="transactionIds">Transaction IDs to reconcile</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of transactions reconciled</returns>
    Task<int> ReconcileTransactionsAsync(IEnumerable<int> transactionIds, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches transactions by description or notes
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="accountId">Optional account filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching transactions</returns>
    Task<IEnumerable<Transaction>> SearchTransactionsAsync(string searchTerm, int? accountId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets transaction summary for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="accountId">Optional account filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction summary</returns>
    Task<TransactionSummary> GetTransactionSummaryAsync(DateTime startDate, DateTime endDate, int? accountId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates a transaction against business rules
    /// </summary>
    /// <param name="transaction">Transaction to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
}

/// <summary>
/// Transaction summary information
/// </summary>
public class TransactionSummary
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public Dictionary<int, decimal> SpendingByCategory { get; set; } = new();
    public Dictionary<TransactionType, int> TransactionsByType { get; set; } = new();
}

/// <summary>
/// Validation result for business rule validation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);
}

