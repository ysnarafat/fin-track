using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Exceptions;
using FinTrack.Core.Interfaces;
using FinTrack.Shared.Models;
using Microsoft.Extensions.Logging;

namespace FinTrack.Shared.Services;

/// <summary>
/// Service implementation for account business logic and operations
/// </summary>
public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IGoalRepository _goalRepository;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IGoalRepository goalRepository,
        ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _goalRepository = goalRepository;
        _logger = logger;
    }

    public async Task<Account> CreateAccountAsync(Account account, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating account: {AccountName} of type {AccountType}", account.Name, account.Type);

        // Validate the account
        var validationResult = await ValidateAccountAsync(account, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new BusinessRuleException("AccountValidationFailed", $"Account validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        // Set initial balance
        account.InitialBalance = account.Balance;

        // Create the account
        var createdAccount = await _accountRepository.AddAsync(account, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account created successfully with ID: {AccountId}", createdAccount.Id);
        return createdAccount;
    }

    public async Task<Account> UpdateAccountAsync(Account account, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating account ID: {AccountId}", account.Id);

        // Validate the account
        var validationResult = await ValidateAccountAsync(account, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new BusinessRuleException("AccountValidationFailed", $"Account validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        // Update the account
        var updatedAccount = await _accountRepository.UpdateAsync(account, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account updated successfully: {AccountId}", account.Id);
        return updatedAccount;
    }

    public async Task<bool> DeleteAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting account ID: {AccountId}", accountId);

        var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found for deletion: {AccountId}", accountId);
            return false;
        }

        // Check if account has transactions
        var transactions = await _transactionRepository.GetByAccountIdAsync(accountId, cancellationToken);
        if (transactions.Any())
        {
            throw new BusinessRuleException("AccountHasTransactions", $"Cannot delete account {account.Name} because it has associated transactions");
        }

        // Check if account has linked goals
        var goals = await _goalRepository.GetByLinkedAccountAsync(accountId, cancellationToken);
        if (goals.Any())
        {
            throw new BusinessRuleException("AccountHasLinkedGoals", $"Cannot delete account {account.Name} because it has linked goals");
        }

        var result = await _accountRepository.DeleteAsync(accountId, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account deleted successfully: {AccountId}", accountId);
        return result;
    }

    public async Task<Account?> GetAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await _accountRepository.GetByIdAsync(accountId, cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _accountRepository.GetActiveAccountsAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetAccountsByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default)
    {
        return await _accountRepository.GetByTypeAsync(accountType, cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetAccountsWithRecentTransactionsAsync(int transactionCount = 5, CancellationToken cancellationToken = default)
    {
        return await _accountRepository.GetWithRecentTransactionsAsync(transactionCount, cancellationToken);
    }

    public async Task<decimal> RecalculateAccountBalanceAsync(int accountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recalculating balance for account ID: {AccountId}", accountId);

        var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
        if (account == null)
        {
            throw new BusinessRuleException("AccountNotFound", $"Account {accountId} not found");
        }

        // Calculate balance from transactions
        var calculatedBalance = await _accountRepository.CalculateBalanceAsync(accountId, cancellationToken);
        
        // Add initial balance
        var totalBalance = account.InitialBalance + calculatedBalance;

        // Update the account balance
        await _accountRepository.UpdateBalanceAsync(accountId, totalBalance, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account balance recalculated: {AccountId} = {Balance}", accountId, totalBalance);
        return totalBalance;
    }

    public async Task<Dictionary<DateTime, decimal>> GetBalanceHistoryAsync(int accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _accountRepository.GetBalanceHistoryAsync(accountId, startDate, endDate, cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetOverdrawnAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _accountRepository.GetOverdrawnAccountsAsync(cancellationToken);
    }

    public async Task<FinancialSummary> GetFinancialSummaryAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating financial summary");

        var accounts = await _accountRepository.GetActiveAccountsAsync(cancellationToken);
        var accountList = accounts.ToList();

        var summary = new FinancialSummary
        {
            TotalAssets = await _accountRepository.GetTotalAssetsAsync(cancellationToken),
            TotalLiabilities = await _accountRepository.GetTotalLiabilitiesAsync(cancellationToken),
            ActiveAccountCount = accountList.Count,
            OverdrawnAccountCount = accountList.Count(a => a.IsOverdrawn)
        };

        summary.NetWorth = summary.TotalAssets - summary.TotalLiabilities;

        // Calculate totals by account type
        foreach (var accountType in Enum.GetValues<AccountType>())
        {
            var accountsOfType = accountList.Where(a => a.Type == accountType).ToList();
            var totalBalance = accountsOfType.Sum(a => a.Balance);
            var accountCount = accountsOfType.Count;

            if (accountCount > 0)
            {
                summary.BalancesByType[accountType] = totalBalance;
                summary.AccountCountByType[accountType] = accountCount;
            }

            // Set specific totals
            switch (accountType)
            {
                case AccountType.Checking:
                case AccountType.Cash:
                    summary.TotalCashAndChecking += totalBalance;
                    break;
                case AccountType.Savings:
                    summary.TotalSavings += totalBalance;
                    break;
                case AccountType.Investment:
                    summary.TotalInvestments += totalBalance;
                    break;
                case AccountType.CreditCard:
                    summary.TotalCreditCardDebt += Math.Abs(Math.Min(totalBalance, 0));
                    break;
                case AccountType.Loan:
                    summary.TotalLoans += Math.Abs(Math.Min(totalBalance, 0));
                    break;
            }
        }

        _logger.LogInformation("Financial summary generated: NetWorth={NetWorth}, Assets={Assets}, Liabilities={Liabilities}", 
            summary.NetWorth, summary.TotalAssets, summary.TotalLiabilities);

        return summary;
    }

    public async Task<AccountSummary?> GetAccountSummaryAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await _accountRepository.GetAccountSummaryAsync(accountId, cancellationToken);
    }

    public async Task<IEnumerable<Account>> SearchAccountsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Enumerable.Empty<Account>();
        }

        return await _accountRepository.SearchAsync(searchTerm, cancellationToken);
    }

    public async Task<bool> SetAccountActiveStatusAsync(int accountId, bool isActive, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting account {AccountId} active status to {IsActive}", accountId, isActive);

        var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found: {AccountId}", accountId);
            return false;
        }

        account.IsActive = isActive;
        await _accountRepository.UpdateAsync(account, cancellationToken);
        await _accountRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account active status updated: {AccountId} = {IsActive}", accountId, isActive);
        return true;
    }

    public async Task<BusinessValidationResult> ValidateAccountAsync(Account account, CancellationToken cancellationToken = default)
    {
        var result = new BusinessValidationResult { IsValid = true };

        // Basic validation
        if (!account.IsValid())
        {
            result.IsValid = false;
            result.AddError("Account data is invalid");
        }

        // Check for duplicate account names
        var existingAccounts = await _accountRepository.GetAllAsync(cancellationToken);
        var duplicateName = existingAccounts.Any(a => 
            a.Id != account.Id && 
            string.Equals(a.Name.Trim(), account.Name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (duplicateName)
        {
            result.IsValid = false;
            result.AddError($"An account with the name '{account.Name}' already exists");
        }

        // Credit card specific validation
        if (account.Type == AccountType.CreditCard)
        {
            if (!account.CreditLimit.HasValue || account.CreditLimit.Value <= 0)
            {
                result.AddWarning("Credit card accounts should have a credit limit set");
            }

            if (account.Balance > 0)
            {
                result.AddWarning("Credit card accounts typically have negative balances (representing debt)");
            }
        }

        // Loan specific validation
        if (account.Type == AccountType.Loan)
        {
            if (account.Balance > 0)
            {
                result.AddWarning("Loan accounts typically have negative balances (representing debt)");
            }
        }

        // Investment account validation
        if (account.Type == AccountType.Investment)
        {
            if (account.InterestRate.HasValue && account.InterestRate.Value < 0)
            {
                result.IsValid = false;
                result.AddError("Interest rate cannot be negative");
            }
        }

        // Business rule validations
        if (account.Balance < -1000000 || account.Balance > 1000000)
        {
            result.AddWarning("Account balance seems unusually high or low");
        }

        return result;
    }
}
