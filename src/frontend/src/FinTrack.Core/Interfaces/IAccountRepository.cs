using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Repository interface for Account-specific operations
/// </summary>
public interface IAccountRepository : IRepository<Account>
{
    /// <summary>
    /// Gets accounts by type
    /// </summary>
    /// <param name="accountType">Account type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of accounts of the specified type</returns>
    Task<IEnumerable<Account>> GetByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets active accounts only
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active accounts</returns>
    Task<IEnumerable<Account>> GetActiveAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts by currency
    /// </summary>
    /// <param name="currency">Currency code (e.g., USD, EUR)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of accounts with the specified currency</returns>
    Task<IEnumerable<Account>> GetByCurrencyAsync(string currency, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts by institution
    /// </summary>
    /// <param name="institution">Institution name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of accounts from the specified institution</returns>
    Task<IEnumerable<Account>> GetByInstitutionAsync(string institution, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets account by account number
    /// </summary>
    /// <param name="accountNumber">Account number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account if found, null otherwise</returns>
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts with low balance (below specified threshold)
    /// </summary>
    /// <param name="threshold">Balance threshold</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of accounts with balance below threshold</returns>
    Task<IEnumerable<Account>> GetLowBalanceAccountsAsync(decimal threshold, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets overdrawn accounts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of overdrawn accounts</returns>
    Task<IEnumerable<Account>> GetOverdrawnAccountsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts with transactions in a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of accounts with transactions in the date range</returns>
    Task<IEnumerable<Account>> GetAccountsWithTransactionsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculates total balance across all accounts
    /// </summary>
    /// <param name="accountType">Optional account type to filter by</param>
    /// <param name="currency">Optional currency to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total balance</returns>
    Task<decimal> CalculateTotalBalanceAsync(AccountType? accountType = null, string? currency = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculates net worth (assets minus liabilities)
    /// </summary>
    /// <param name="currency">Optional currency to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Net worth amount</returns>
    Task<decimal> CalculateNetWorthAsync(string? currency = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets account balance history for a specific period
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of date to balance amount</returns>
    Task<Dictionary<DateTime, decimal>> GetBalanceHistoryAsync(int accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates account balance by applying a transaction amount
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="amount">Amount to add/subtract from balance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated account</returns>
    Task<Account?> UpdateBalanceAsync(int accountId, decimal amount, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Bulk updates account balances
    /// </summary>
    /// <param name="balanceUpdates">Dictionary of account ID to balance change amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of accounts updated</returns>
    Task<int> BulkUpdateBalancesAsync(Dictionary<int, decimal> balanceUpdates, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts summary with basic statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary containing account statistics</returns>
    Task<Dictionary<string, object>> GetAccountsSummaryAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches accounts by name or institution
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of accounts matching the search term</returns>
    Task<IEnumerable<Account>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts ordered by balance (descending by default)
    /// </summary>
    /// <param name="ascending">True for ascending order, false for descending</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of accounts ordered by balance</returns>
    Task<IEnumerable<Account>> GetOrderedByBalanceAsync(bool ascending = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates account balance against transaction history
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if balance is correct, false if there's a discrepancy</returns>
    Task<bool> ValidateBalanceAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Recalculates account balance from transaction history
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recalculated balance amount</returns>
    Task<decimal> RecalculateBalanceAsync(int accountId, CancellationToken cancellationToken = default);
}