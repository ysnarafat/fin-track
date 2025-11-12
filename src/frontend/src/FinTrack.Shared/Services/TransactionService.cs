using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Exceptions;
using FinTrack.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinTrack.Shared.Services;

/// <summary>
/// Service implementation for transaction business logic and operations
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        IBudgetRepository budgetRepository,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _budgetRepository = budgetRepository;
        _logger = logger;
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating transaction: {Description} for amount {Amount}", transaction.Description, transaction.Amount);

        // Validate the transaction
        var validationResult = await ValidateTransactionAsync(transaction, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new BusinessRuleException("TransactionValidationFailed", $"Transaction validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        // Create the transaction
        var createdTransaction = await _transactionRepository.AddAsync(transaction, cancellationToken);
        
        // Update account balance
        await UpdateAccountBalanceAsync(transaction.AccountId, transaction.SignedAmount, cancellationToken);
        
        // Update budget spending if applicable
        if (transaction.Type == TransactionType.Expense)
        {
            await UpdateBudgetSpendingAsync(transaction.CategoryId, transaction.Amount, cancellationToken);
        }

        await _transactionRepository.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Transaction created successfully with ID: {TransactionId}", createdTransaction.Id);
        return createdTransaction;
    }

    public async Task<Transaction> UpdateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating transaction ID: {TransactionId}", transaction.Id);

        // Get the original transaction to calculate balance adjustments
        var originalTransaction = await _transactionRepository.GetByIdAsync(transaction.Id, cancellationToken);
        if (originalTransaction == null)
        {
            throw new BusinessRuleException("ValidationError", $"Transaction with ID {transaction.Id} not found");
        }

        // Validate the updated transaction
        var validationResult = await ValidateTransactionAsync(transaction, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new BusinessRuleException("TransactionValidationFailed", $"Transaction validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        // Reverse the original transaction's effect on account balance
        await UpdateAccountBalanceAsync(originalTransaction.AccountId, -originalTransaction.SignedAmount, cancellationToken);
        
        // Reverse budget spending if applicable
        if (originalTransaction.Type == TransactionType.Expense)
        {
            await UpdateBudgetSpendingAsync(originalTransaction.CategoryId, -originalTransaction.Amount, cancellationToken);
        }

        // Update the transaction
        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        
        // Apply the new transaction's effect on account balance
        await UpdateAccountBalanceAsync(transaction.AccountId, transaction.SignedAmount, cancellationToken);
        
        // Update budget spending if applicable
        if (transaction.Type == TransactionType.Expense)
        {
            await UpdateBudgetSpendingAsync(transaction.CategoryId, transaction.Amount, cancellationToken);
        }

        await _transactionRepository.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Transaction updated successfully: {TransactionId}", transaction.Id);
        return updatedTransaction;
    }

    public async Task<bool> DeleteTransactionAsync(int transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting transaction ID: {TransactionId}", transactionId);

        var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
        if (transaction == null)
        {
            _logger.LogWarning("Transaction not found for deletion: {TransactionId}", transactionId);
            return false;
        }

        // Reverse the transaction's effect on account balance
        await UpdateAccountBalanceAsync(transaction.AccountId, -transaction.SignedAmount, cancellationToken);
        
        // Reverse budget spending if applicable
        if (transaction.Type == TransactionType.Expense)
        {
            await UpdateBudgetSpendingAsync(transaction.CategoryId, -transaction.Amount, cancellationToken);
        }

        // Handle linked transactions for transfers
        if (transaction.LinkedTransactionId.HasValue)
        {
            var linkedTransaction = await _transactionRepository.GetByIdAsync(transaction.LinkedTransactionId.Value, cancellationToken);
            if (linkedTransaction != null)
            {
                await _transactionRepository.DeleteAsync(linkedTransaction.Id, cancellationToken);
                await UpdateAccountBalanceAsync(linkedTransaction.AccountId, -linkedTransaction.SignedAmount, cancellationToken);
            }
        }

        var result = await _transactionRepository.DeleteAsync(transactionId, cancellationToken);
        await _transactionRepository.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Transaction deleted successfully: {TransactionId}", transactionId);
        return result;
    }

    public async Task<Transaction?> GetTransactionAsync(int transactionId, CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(
        int? accountId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? categoryId = null,
        TransactionType? transactionType = null,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetAllAsync(cancellationToken);

        // Apply filters
        if (accountId.HasValue)
        {
            transactions = transactions.Where(t => t.AccountId == accountId.Value);
        }

        if (startDate.HasValue)
        {
            transactions = transactions.Where(t => t.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            transactions = transactions.Where(t => t.Date <= endDate.Value);
        }

        if (categoryId.HasValue)
        {
            transactions = transactions.Where(t => t.CategoryId == categoryId.Value);
        }

        if (transactionType.HasValue)
        {
            transactions = transactions.Where(t => t.Type == transactionType.Value);
        }

        return transactions.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id);
    }

    public async Task<(Transaction sourceTransaction, Transaction destinationTransaction)> CreateTransferAsync(
        int fromAccountId,
        int toAccountId,
        decimal amount,
        string description,
        DateTime date,
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating transfer from account {FromAccountId} to {ToAccountId} for amount {Amount}", 
            fromAccountId, toAccountId, amount);

        if (fromAccountId == toAccountId)
        {
            throw new BusinessRuleException("ValidationError", "Cannot transfer to the same account");
        }

        if (amount <= 0)
        {
            throw new BusinessRuleException("ValidationError", "Transfer amount must be greater than zero");
        }

        // Verify accounts exist
        var fromAccount = await _accountRepository.GetByIdAsync(fromAccountId, cancellationToken);
        var toAccount = await _accountRepository.GetByIdAsync(toAccountId, cancellationToken);

        if (fromAccount == null)
        {
            throw new BusinessRuleException("ValidationError", $"Source account {fromAccountId} not found");
        }

        if (toAccount == null)
        {
            throw new BusinessRuleException("ValidationError", $"Destination account {toAccountId} not found");
        }

        // Create source transaction (outgoing)
        var sourceTransaction = new Transaction
        {
            Amount = amount,
            Description = $"{description} (to {toAccount.Name})",
            Date = date,
            CategoryId = categoryId,
            AccountId = fromAccountId,
            Type = TransactionType.Transfer,
            TransferToAccountId = toAccountId
        };

        // Create destination transaction (incoming)
        var destinationTransaction = new Transaction
        {
            Amount = amount,
            Description = $"{description} (from {fromAccount.Name})",
            Date = date,
            CategoryId = categoryId,
            AccountId = toAccountId,
            Type = TransactionType.Transfer,
            TransferToAccountId = fromAccountId
        };

        // Add transactions
        var createdSource = await _transactionRepository.AddAsync(sourceTransaction, cancellationToken);
        var createdDestination = await _transactionRepository.AddAsync(destinationTransaction, cancellationToken);

        // Link the transactions
        createdSource.LinkedTransactionId = createdDestination.Id;
        createdDestination.LinkedTransactionId = createdSource.Id;

        await _transactionRepository.UpdateAsync(createdSource, cancellationToken);
        await _transactionRepository.UpdateAsync(createdDestination, cancellationToken);

        // Update account balances
        await UpdateAccountBalanceAsync(fromAccountId, -amount, cancellationToken);
        await UpdateAccountBalanceAsync(toAccountId, amount, cancellationToken);

        await _transactionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transfer created successfully between accounts {FromAccountId} and {ToAccountId}", 
            fromAccountId, toAccountId);

        return (createdSource, createdDestination);
    }

    public async Task<int> ReconcileTransactionsAsync(IEnumerable<int> transactionIds, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reconciling {Count} transactions", transactionIds.Count());

        var reconciledCount = await _transactionRepository.BulkReconcileAsync(transactionIds, DateTime.UtcNow, cancellationToken);
        await _transactionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reconciled {Count} transactions", reconciledCount);
        return reconciledCount;
    }

    public async Task<IEnumerable<Transaction>> SearchTransactionsAsync(string searchTerm, int? accountId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Enumerable.Empty<Transaction>();
        }

        var transactions = await _transactionRepository.SearchByDescriptionAsync(searchTerm, cancellationToken);

        if (accountId.HasValue)
        {
            transactions = transactions.Where(t => t.AccountId == accountId.Value);
        }

        return transactions.OrderByDescending(t => t.Date);
    }

    public async Task<TransactionSummary> GetTransactionSummaryAsync(DateTime startDate, DateTime endDate, int? accountId = null, CancellationToken cancellationToken = default)
    {
        var transactions = await GetTransactionsAsync(accountId, startDate, endDate, cancellationToken: cancellationToken);
        var transactionList = transactions.ToList();

        var summary = new TransactionSummary
        {
            TotalIncome = transactionList.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpenses = transactionList.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
            TransactionCount = transactionList.Count,
            SpendingByCategory = transactionList
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.CategoryId)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount)),
            TransactionsByType = transactionList
                .GroupBy(t => t.Type)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        summary.NetAmount = summary.TotalIncome - summary.TotalExpenses;
        summary.AverageTransactionAmount = summary.TransactionCount > 0 
            ? transactionList.Sum(t => t.Amount) / summary.TransactionCount 
            : 0;

        return summary;
    }

    public async Task<ValidationResult> ValidateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        // Basic validation
        if (!transaction.IsValid())
        {
            result.IsValid = false;
            result.AddError("Transaction data is invalid");
        }

        // Verify account exists and is active
        var account = await _accountRepository.GetByIdAsync(transaction.AccountId, cancellationToken);
        if (account == null)
        {
            result.IsValid = false;
            result.AddError($"Account {transaction.AccountId} not found");
        }
        else if (!account.IsActive)
        {
            result.IsValid = false;
            result.AddError($"Account {account.Name} is not active");
        }

        // Verify category exists
        var category = await _categoryRepository.GetByIdAsync(transaction.CategoryId, cancellationToken);
        if (category == null)
        {
            result.IsValid = false;
            result.AddError($"Category {transaction.CategoryId} not found");
        }

        // Transfer-specific validation
        if (transaction.Type == TransactionType.Transfer)
        {
            if (!transaction.TransferToAccountId.HasValue)
            {
                result.IsValid = false;
                result.AddError("Transfer destination account is required");
            }
            else
            {
                var toAccount = await _accountRepository.GetByIdAsync(transaction.TransferToAccountId.Value, cancellationToken);
                if (toAccount == null)
                {
                    result.IsValid = false;
                    result.AddError($"Transfer destination account {transaction.TransferToAccountId} not found");
                }
                else if (!toAccount.IsActive)
                {
                    result.IsValid = false;
                    result.AddError($"Transfer destination account {toAccount.Name} is not active");
                }
            }
        }

        // Business rule validations
        if (transaction.Date > DateTime.Today.AddDays(7))
        {
            result.AddWarning("Transaction date is more than a week in the future");
        }

        if (account != null && transaction.Type == TransactionType.Expense)
        {
            var newBalance = account.Balance - transaction.Amount;
            if (newBalance < 0 && account.Type != AccountType.CreditCard)
            {
                result.AddWarning($"This transaction will cause account {account.Name} to be overdrawn");
            }
        }

        return result;
    }

    private async Task UpdateAccountBalanceAsync(int accountId, decimal amount, CancellationToken cancellationToken)
    {
        await _accountRepository.UpdateBalanceAsync(accountId, 
            (await _accountRepository.GetByIdAsync(accountId, cancellationToken))!.Balance + amount, 
            cancellationToken);
    }

    private async Task UpdateBudgetSpendingAsync(int categoryId, decimal amount, CancellationToken cancellationToken)
    {
        var budgets = await _budgetRepository.GetByCategoryAsync(categoryId, cancellationToken);
        var currentBudgets = budgets.Where(b => b.IsActive && b.IsCurrentPeriod);

        foreach (var budget in currentBudgets)
        {
            if (amount > 0)
            {
                budget.AddSpending(amount);
            }
            else
            {
                budget.RemoveSpending(Math.Abs(amount));
            }
            
            await _budgetRepository.UpdateAsync(budget, cancellationToken);
        }
    }
}



