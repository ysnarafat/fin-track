using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Interfaces;
using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Account-specific operations
/// </summary>
public class AccountRepository : BaseRepository<Account>, IAccountRepository
{
    /// <summary>
    /// Constructor for AccountRepository
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    public AccountRepository(FinTrackDbContext context, ILogger<AccountRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Gets accounts by type
    /// </summary>
    public async Task<IEnumerable<Account>> GetByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting accounts of type {AccountType}", accountType);
            
            var accounts = await _dbSet
                .Where(a => a.Type == accountType && !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} accounts of type {AccountType}", accounts.Count, accountType);
            return accounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accounts of type {AccountType}", accountType);
            throw;
        }
    }

    /// <summary>
    /// Gets active accounts only
    /// </summary>
    public async Task<IEnumerable<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting active accounts");
            
            var accounts = await _dbSet
                .Where(a => a.IsActive && !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} active accounts", accounts.Count);
            return accounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active accounts");
            throw;
        }
    }

    /// <summary>
    /// Gets accounts with their recent transactions
    /// </summary>
    public async Task<IEnumerable<Account>> GetWithRecentTransactionsAsync(int transactionCount = 5, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting accounts with {TransactionCount} recent transactions", transactionCount);
            
            var accounts = await _dbSet
                .Where(a => !a.IsDeleted)
                .Include(a => a.Transactions.Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .Take(transactionCount))
                .ThenInclude(t => t.Category)
                .OrderBy(a => a.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} accounts with recent transactions", accounts.Count);
            return accounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accounts with recent transactions");
            throw;
        }
    }

    /// <summary>
    /// Calculates the current balance for an account based on transactions
    /// </summary>
    public async Task<decimal> CalculateBalanceAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Calculating balance for account {AccountId}", accountId);
            
            var account = await _dbSet
                .Where(a => a.Id == accountId && !a.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (account == null)
            {
                _logger.LogWarning("Account {AccountId} not found for balance calculation", accountId);
                return 0;
            }
            
            // Start with initial balance
            var calculatedBalance = account.InitialBalance;
            
            // Add all income transactions
            var income = await _context.Transactions
                .Where(t => t.AccountId == accountId && t.Type == TransactionType.Income && !t.IsDeleted)
                .SumAsync(t => t.Amount, cancellationToken);
                
            // Subtract all expense transactions
            var expenses = await _context.Transactions
                .Where(t => t.AccountId == accountId && t.Type == TransactionType.Expense && !t.IsDeleted)
                .SumAsync(t => t.Amount, cancellationToken);
                
            // Handle transfer transactions
            var transfersOut = await _context.Transactions
                .Where(t => t.AccountId == accountId && t.Type == TransactionType.Transfer && !t.IsDeleted)
                .SumAsync(t => t.Amount, cancellationToken);
                
            var transfersIn = await _context.Transactions
                .Where(t => t.TransferToAccountId == accountId && t.Type == TransactionType.Transfer && !t.IsDeleted)
                .SumAsync(t => t.Amount, cancellationToken);
            
            calculatedBalance = calculatedBalance + income - expenses - transfersOut + transfersIn;
            
            _logger.LogDebug("Calculated balance for account {AccountId}: {Balance}", accountId, calculatedBalance);
            return calculatedBalance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating balance for account {AccountId}", accountId);
            throw;
        }
    }

    /// <summary>
    /// Updates account balance and marks as modified
    /// </summary>
    public async Task<bool> UpdateBalanceAsync(int accountId, decimal newBalance, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating balance for account {AccountId} to {NewBalance}", accountId, newBalance);
            
            var account = await _dbSet
                .Where(a => a.Id == accountId && !a.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (account == null)
            {
                _logger.LogWarning("Account {AccountId} not found for balance update", accountId);
                return false;
            }
            
            account.SetBalance(newBalance);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Updated balance for account {AccountId} to {NewBalance}", accountId, newBalance);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance for account {AccountId}", accountId);
            throw;
        }
    }

    /// <summary>
    /// Gets account balance history for a date range
    /// </summary>
    public async Task<Dictionary<DateTime, decimal>> GetBalanceHistoryAsync(int accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting balance history for account {AccountId} from {StartDate} to {EndDate}", 
                accountId, startDate, endDate);
            
            var account = await _dbSet
                .Where(a => a.Id == accountId && !a.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (account == null)
            {
                _logger.LogWarning("Account {AccountId} not found for balance history", accountId);
                return new Dictionary<DateTime, decimal>();
            }
            
            // Get all transactions for the account up to the end date, ordered by date
            var transactions = await _context.Transactions
                .Where(t => (t.AccountId == accountId || t.TransferToAccountId == accountId) && 
                           t.Date <= endDate && 
                           !t.IsDeleted)
                .OrderBy(t => t.Date)
                .ThenBy(t => t.Id)
                .ToListAsync(cancellationToken);
            
            var balanceHistory = new Dictionary<DateTime, decimal>();
            var runningBalance = account.InitialBalance;
            
            // Calculate balance for each day in the range
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // Process all transactions for this date
                var dayTransactions = transactions.Where(t => t.Date.Date == date).ToList();
                
                foreach (var transaction in dayTransactions)
                {
                    if (transaction.AccountId == accountId)
                    {
                        // Transaction from this account
                        switch (transaction.Type)
                        {
                            case TransactionType.Income:
                                runningBalance += transaction.Amount;
                                break;
                            case TransactionType.Expense:
                                runningBalance -= transaction.Amount;
                                break;
                            case TransactionType.Transfer:
                                runningBalance -= transaction.Amount; // Money leaving the account
                                break;
                        }
                    }
                    else if (transaction.TransferToAccountId == accountId)
                    {
                        // Transfer to this account
                        runningBalance += transaction.Amount;
                    }
                }
                
                balanceHistory[date] = runningBalance;
            }
            
            _logger.LogDebug("Retrieved balance history for {Days} days for account {AccountId}", 
                balanceHistory.Count, accountId);
            return balanceHistory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance history for account {AccountId}", accountId);
            throw;
        }
    }

    /// <summary>
    /// Gets accounts that are overdrawn or over credit limit
    /// </summary>
    public async Task<IEnumerable<Account>> GetOverdrawnAccountsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting overdrawn accounts");
            
            var accounts = await _dbSet
                .Where(a => !a.IsDeleted && a.IsActive)
                .ToListAsync(cancellationToken);
            
            var overdrawnAccounts = accounts.Where(a => a.IsOverdrawn).ToList();
            
            _logger.LogDebug("Found {Count} overdrawn accounts", overdrawnAccounts.Count);
            return overdrawnAccounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdrawn accounts");
            throw;
        }
    }

    /// <summary>
    /// Gets total net worth (sum of all account balances considering account types)
    /// </summary>
    public async Task<decimal> GetTotalNetWorthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Calculating total net worth");
            
            var accounts = await _dbSet
                .Where(a => !a.IsDeleted && a.IsActive)
                .ToListAsync(cancellationToken);
            
            var totalNetWorth = accounts.Sum(a => a.Type switch
            {
                AccountType.CreditCard => -Math.Abs(a.Balance), // Credit card debt is negative
                AccountType.Loan => -Math.Abs(a.Balance), // Loan balance is negative
                _ => a.Balance // Assets are positive
            });
            
            _logger.LogDebug("Calculated total net worth: {NetWorth}", totalNetWorth);
            return totalNetWorth;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total net worth");
            throw;
        }
    }

    /// <summary>
    /// Gets total assets (positive balance accounts)
    /// </summary>
    public async Task<decimal> GetTotalAssetsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Calculating total assets");
            
            var totalAssets = await _dbSet
                .Where(a => !a.IsDeleted && a.IsActive && 
                           (a.Type == AccountType.Checking || 
                            a.Type == AccountType.Savings || 
                            a.Type == AccountType.Investment))
                .SumAsync(a => a.Balance, cancellationToken);
            
            _logger.LogDebug("Calculated total assets: {Assets}", totalAssets);
            return totalAssets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total assets");
            throw;
        }
    }

    /// <summary>
    /// Gets total liabilities (negative balance accounts and credit card debt)
    /// </summary>
    public async Task<decimal> GetTotalLiabilitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Calculating total liabilities");
            
            var totalLiabilities = await _dbSet
                .Where(a => !a.IsDeleted && a.IsActive && 
                           (a.Type == AccountType.CreditCard || 
                            a.Type == AccountType.Loan))
                .SumAsync(a => Math.Abs(a.Balance), cancellationToken);
            
            _logger.LogDebug("Calculated total liabilities: {Liabilities}", totalLiabilities);
            return totalLiabilities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total liabilities");
            throw;
        }
    }

    /// <summary>
    /// Searches accounts by name or institution
    /// </summary>
    public async Task<IEnumerable<Account>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Account>();
                
            _logger.LogDebug("Searching accounts by term: {SearchTerm}", searchTerm);
            
            var accounts = await _dbSet
                .Where(a => (a.Name.Contains(searchTerm) || 
                            (a.Institution != null && a.Institution.Contains(searchTerm))) && 
                           !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Found {Count} accounts matching search term: {SearchTerm}", accounts.Count, searchTerm);
            return accounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching accounts by term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Gets account summary with transaction counts and date ranges
    /// </summary>
    public async Task<AccountSummary?> GetAccountSummaryAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting account summary for account {AccountId}", accountId);
            
            var account = await _dbSet
                .Where(a => a.Id == accountId && !a.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (account == null)
            {
                _logger.LogWarning("Account {AccountId} not found for summary", accountId);
                return null;
            }
            
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);
            
            // Get transaction statistics
            var transactionStats = await _context.Transactions
                .Where(t => t.AccountId == accountId && !t.IsDeleted)
                .GroupBy(t => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    FirstDate = g.Min(t => t.Date),
                    LastDate = g.Max(t => t.Date)
                })
                .FirstOrDefaultAsync(cancellationToken);
            
            // Get month-to-date income and expenses
            var mtdIncome = await _context.Transactions
                .Where(t => t.AccountId == accountId && 
                           t.Type == TransactionType.Income && 
                           t.Date >= monthStart && 
                           !t.IsDeleted)
                .SumAsync(t => t.Amount, cancellationToken);
                
            var mtdExpenses = await _context.Transactions
                .Where(t => t.AccountId == accountId && 
                           t.Type == TransactionType.Expense && 
                           t.Date >= monthStart && 
                           !t.IsDeleted)
                .SumAsync(t => t.Amount, cancellationToken);
            
            // Get year-to-date income and expenses
            var ytdIncome = await _context.Transactions
                .Where(t => t.AccountId == accountId && 
                           t.Type == TransactionType.Income && 
                           t.Date >= yearStart && 
                           !t.IsDeleted)
                .SumAsync(t => t.Amount, cancellationToken);
                
            var ytdExpenses = await _context.Transactions
                .Where(t => t.AccountId == accountId && 
                           t.Type == TransactionType.Expense && 
                           t.Date >= yearStart && 
                           !t.IsDeleted)
                .SumAsync(t => t.Amount, cancellationToken);
            
            var summary = new AccountSummary
            {
                AccountId = account.Id,
                AccountName = account.Name,
                CurrentBalance = account.Balance,
                AvailableBalance = account.AvailableBalance,
                TransactionCount = transactionStats?.Count ?? 0,
                FirstTransactionDate = transactionStats?.FirstDate,
                LastTransactionDate = transactionStats?.LastDate,
                MonthToDateIncome = mtdIncome,
                MonthToDateExpenses = mtdExpenses,
                YearToDateIncome = ytdIncome,
                YearToDateExpenses = ytdExpenses
            };
            
            _logger.LogDebug("Retrieved account summary for account {AccountId}", accountId);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account summary for account {AccountId}", accountId);
            throw;
        }
    }

    /// <summary>
    /// Gets an account by name
    /// </summary>
    public async Task<Account?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting account by name: {Name}", name);
            
            var account = await _dbSet
                .Where(a => !a.IsDeleted && a.Name == name)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (account == null)
            {
                _logger.LogDebug("Account with name {Name} not found", name);
            }
            
            return account;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account by name: {Name}", name);
            throw;
        }
    }

    /// <summary>
    /// Gets a transaction by ID
    /// </summary>
    public async Task<Transaction?> GetTransactionByIdAsync(int transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting transaction by ID: {TransactionId}", transactionId);
            
            var transaction = await _context.Set<Transaction>()
                .Where(t => t.Id == transactionId && !t.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
            
            if (transaction == null)
            {
                _logger.LogDebug("Transaction with ID {TransactionId} not found", transactionId);
            }
            
            return transaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction by ID: {TransactionId}", transactionId);
            throw;
        }
    }
}