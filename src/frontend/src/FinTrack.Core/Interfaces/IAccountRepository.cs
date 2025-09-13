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
    Task<IEnumerable<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts with their recent transactions
    /// </summary>
    /// <param name="transactionCount">Number of recent transactions to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of accounts with recent transactions</returns>
    Task<IEnumerable<Account>> GetWithRecentTransactionsAsync(int transactionCount = 5, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculates the current balance for an account based on transactions
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Calculated balance</returns>
    Task<decimal> CalculateBalanceAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates account balance and marks as modified
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="newBalance">New balance amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateBalanceAsync(int accountId, decimal newBalance, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets account balance history for a date range
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of date to balance amount</returns>
    Task<Dictionary<DateTime, decimal>> GetBalanceHistoryAsync(int accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts that are overdrawn or over credit limit
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of overdrawn accounts</returns>
    Task<IEnumerable<Account>> GetOverdrawnAccountsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets total net worth (sum of all account balances considering account types)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total net worth</returns>
    Task<decimal> GetTotalNetWorthAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets total assets (positive balance accounts)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total assets amount</returns>
    Task<decimal> GetTotalAssetsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets total liabilities (negative balance accounts and credit card debt)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total liabilities amount</returns>
    Task<decimal> GetTotalLiabilitiesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches accounts by name or institution
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching accounts</returns>
    Task<IEnumerable<Account>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets account summary with transaction counts and date ranges
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account summary information</returns>
    Task<AccountSummary?> GetAccountSummaryAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an account by name
    /// </summary>
    /// <param name="name">Account name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account if found, null otherwise</returns>
    Task<Account?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a transaction by ID
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction if found, null otherwise</returns>
    Task<Transaction?> GetTransactionByIdAsync(int transactionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Summary information for an account
/// </summary>
public class AccountSummary
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public int TransactionCount { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public DateTime? FirstTransactionDate { get; set; }
    public decimal MonthToDateIncome { get; set; }
    public decimal MonthToDateExpenses { get; set; }
    public decimal YearToDateIncome { get; set; }
    public decimal YearToDateExpenses { get; set; }
}