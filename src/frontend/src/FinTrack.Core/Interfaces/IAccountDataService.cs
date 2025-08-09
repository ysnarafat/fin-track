using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Data service interface for Account-specific business operations
/// </summary>
public interface IAccountDataService : IDataService<Account>
{
    /// <summary>
    /// Creates a new account with initial balance transaction
    /// </summary>
    /// <param name="account">Account to create</param>
    /// <param name="initialBalanceCategoryId">Category ID for initial balance transaction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created account</returns>
    Task<Account> CreateAccountAsync(Account account, int initialBalanceCategoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets account with transaction history
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="includeTransactions">Whether to include transaction history</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account with optional transaction history</returns>
    Task<Account?> GetAccountWithHistoryAsync(int accountId, bool includeTransactions = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all accounts with their current balances and basic statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of accounts with statistics</returns>
    Task<IEnumerable<AccountSummary>> GetAccountSummariesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates account balance (used internally by transaction operations)
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="amount">Amount to add/subtract</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated account</returns>
    Task<Account?> AdjustBalanceAsync(int accountId, decimal amount, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Recalculates account balance from transaction history
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account with corrected balance</returns>
    Task<Account?> RecalculateBalanceAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets account balance history for charting
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="interval">Data point interval (daily, weekly, monthly)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of balance history points</returns>
    Task<IEnumerable<BalanceHistoryPoint>> GetBalanceHistoryAsync(int accountId, DateTime startDate, DateTime endDate, HistoryInterval interval = HistoryInterval.Daily, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets net worth calculation across all accounts
    /// </summary>
    /// <param name="currency">Optional currency filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Net worth summary</returns>
    Task<NetWorthSummary> GetNetWorthAsync(string? currency = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts that need attention (low balance, overdrawn, etc.)
    /// </summary>
    /// <param name="lowBalanceThreshold">Threshold for low balance alerts</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of account alerts</returns>
    Task<IEnumerable<AccountAlert>> GetAccountAlertsAsync(decimal lowBalanceThreshold = 100m, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Archives an account (marks as inactive and prevents new transactions)
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Archived account</returns>
    Task<Account?> ArchiveAccountAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reactivates an archived account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reactivated account</returns>
    Task<Account?> ReactivateAccountAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets account performance metrics
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="startDate">Start date for analysis</param>
    /// <param name="endDate">End date for analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account performance metrics</returns>
    Task<AccountPerformance> GetAccountPerformanceAsync(int accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Merges two accounts (moves all transactions from source to target)
    /// </summary>
    /// <param name="sourceAccountId">Source account ID (will be archived)</param>
    /// <param name="targetAccountId">Target account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Target account with merged data</returns>
    Task<Account?> MergeAccountsAsync(int sourceAccountId, int targetAccountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets account transaction statistics
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction statistics for the account</returns>
    Task<AccountTransactionStats> GetTransactionStatsAsync(int accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates account data and business rules
    /// </summary>
    /// <param name="account">Account to validate</param>
    /// <param name="isUpdate">Whether this is an update operation</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateAccountAsync(Account account, bool isUpdate = false);
}

/// <summary>
/// Represents an account summary with basic statistics
/// </summary>
public class AccountSummary
{
    public Account Account { get; set; } = null!;
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public int TransactionCount { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpenses { get; set; }
}

/// <summary>
/// Represents a point in account balance history
/// </summary>
public class BalanceHistoryPoint
{
    public DateTime Date { get; set; }
    public decimal Balance { get; set; }
    public decimal Change { get; set; }
}

/// <summary>
/// Represents net worth summary across all accounts
/// </summary>
public class NetWorthSummary
{
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime CalculatedAt { get; set; }
    public List<AccountTypeSummary> AccountTypeSummaries { get; set; } = new();
}

/// <summary>
/// Represents summary by account type
/// </summary>
public class AccountTypeSummary
{
    public AccountType AccountType { get; set; }
    public decimal TotalBalance { get; set; }
    public int AccountCount { get; set; }
}

/// <summary>
/// Represents an account alert
/// </summary>
public class AccountAlert
{
    public Account Account { get; set; } = null!;
    public AccountAlertType AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal? ThresholdValue { get; set; }
    public decimal CurrentValue { get; set; }
}

/// <summary>
/// Types of account alerts
/// </summary>
public enum AccountAlertType
{
    LowBalance,
    Overdrawn,
    OverCreditLimit,
    InactiveAccount,
    HighActivity
}

/// <summary>
/// Represents account performance metrics
/// </summary>
public class AccountPerformance
{
    public Account Account { get; set; } = null!;
    public decimal StartingBalance { get; set; }
    public decimal EndingBalance { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetChange { get; set; }
    public decimal AverageMonthlyIncome { get; set; }
    public decimal AverageMonthlyExpenses { get; set; }
    public int TransactionCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Represents account transaction statistics
/// </summary>
public class AccountTransactionStats
{
    public int TotalTransactions { get; set; }
    public int IncomeTransactions { get; set; }
    public int ExpenseTransactions { get; set; }
    public int TransferTransactions { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public decimal LargestTransaction { get; set; }
    public decimal SmallestTransaction { get; set; }
    public Dictionary<int, int> TransactionsByCategory { get; set; } = new();
    public Dictionary<int, decimal> SpendingByCategory { get; set; } = new();
}

/// <summary>
/// History interval for balance history
/// </summary>
public enum HistoryInterval
{
    Daily,
    Weekly,
    Monthly
}