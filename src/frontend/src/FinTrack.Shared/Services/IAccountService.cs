using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Interfaces;
using FinTrack.Shared.Models;

namespace FinTrack.Shared.Services;

/// <summary>
/// Service interface for account business logic and operations
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Creates a new account with business logic validation
    /// </summary>
    /// <param name="account">Account to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created account</returns>
    Task<Account> CreateAccountAsync(Account account, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing account with business logic validation
    /// </summary>
    /// <param name="account">Account to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated account</returns>
    Task<Account> UpdateAccountAsync(Account account, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an account (soft delete) after validating it can be deleted
    /// </summary>
    /// <param name="accountId">Account ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAccountAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an account by ID
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account if found, null otherwise</returns>
    Task<Account?> GetAccountAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all active accounts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active accounts</returns>
    Task<IEnumerable<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts by type
    /// </summary>
    /// <param name="accountType">Account type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of accounts of the specified type</returns>
    Task<IEnumerable<Account>> GetAccountsByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets accounts with their recent transactions
    /// </summary>
    /// <param name="transactionCount">Number of recent transactions to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of accounts with recent transactions</returns>
    Task<IEnumerable<Account>> GetAccountsWithRecentTransactionsAsync(int transactionCount = 5, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Recalculates and updates account balance based on transactions
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated balance</returns>
    Task<decimal> RecalculateAccountBalanceAsync(int accountId, CancellationToken cancellationToken = default);
    
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
    /// Gets financial summary across all accounts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Financial summary</returns>
    Task<FinancialSummary> GetFinancialSummaryAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets detailed account summary with transaction statistics
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account summary</returns>
    Task<AccountSummary?> GetAccountSummaryAsync(int accountId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches accounts by name or institution
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching accounts</returns>
    Task<IEnumerable<Account>> SearchAccountsAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Activates or deactivates an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="isActive">Active status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> SetAccountActiveStatusAsync(int accountId, bool isActive, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates an account against business rules
    /// </summary>
    /// <param name="account">Account to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<BusinessValidationResult> ValidateAccountAsync(Account account, CancellationToken cancellationToken = default);
}

/// <summary>
/// Financial summary across all accounts
/// </summary>
public class FinancialSummary
{
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public decimal TotalCashAndChecking { get; set; }
    public decimal TotalSavings { get; set; }
    public decimal TotalInvestments { get; set; }
    public decimal TotalCreditCardDebt { get; set; }
    public decimal TotalLoans { get; set; }
    public int ActiveAccountCount { get; set; }
    public int OverdrawnAccountCount { get; set; }
    public Dictionary<AccountType, decimal> BalancesByType { get; set; } = new();
    public Dictionary<AccountType, int> AccountCountByType { get; set; } = new();
}

