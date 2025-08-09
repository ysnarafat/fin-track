using FinTrack.Maui.Models;

namespace FinTrack.Maui.Services;

public class TransactionService : ITransactionService
{
    private readonly List<SimpleTransaction> _transactions;
    private int _nextId = 1;

    public TransactionService()
    {
        // Initialize with sample data
        _transactions = new List<SimpleTransaction>
        {
            new SimpleTransaction
            {
                Id = _nextId++,
                Description = "Grocery Shopping",
                Amount = -85.50m,
                Date = DateTime.Today,
                Category = "Food & Dining",
                Type = SimpleTransactionType.Expense
            },
            new SimpleTransaction
            {
                Id = _nextId++,
                Description = "Monthly Salary",
                Amount = 3500.00m,
                Date = DateTime.Today.AddDays(-1),
                Category = "Income",
                Type = SimpleTransactionType.Income
            },
            new SimpleTransaction
            {
                Id = _nextId++,
                Description = "Gas Station",
                Amount = -45.20m,
                Date = DateTime.Today.AddDays(-2),
                Category = "Transportation",
                Type = SimpleTransactionType.Expense
            },
            new SimpleTransaction
            {
                Id = _nextId++,
                Description = "Coffee Shop",
                Amount = -12.75m,
                Date = DateTime.Today.AddDays(-3),
                Category = "Food & Dining",
                Type = SimpleTransactionType.Expense
            },
            new SimpleTransaction
            {
                Id = _nextId++,
                Description = "Freelance Payment",
                Amount = 750.00m,
                Date = DateTime.Today.AddDays(-5),
                Category = "Income",
                Type = SimpleTransactionType.Income
            }
        };
    }

    public async Task<List<SimpleTransaction>> GetTransactionsAsync()
    {
        // Simulate async operation
        await Task.Delay(100);
        return _transactions.OrderByDescending(t => t.Date).ToList();
    }

    public async Task<SimpleTransaction?> GetTransactionByIdAsync(int id)
    {
        await Task.Delay(50);
        return _transactions.FirstOrDefault(t => t.Id == id);
    }

    public async Task<SimpleTransaction> CreateTransactionAsync(SimpleTransaction transaction)
    {
        await Task.Delay(100);
        
        transaction.Id = _nextId++;
        _transactions.Add(transaction);
        
        return transaction;
    }

    public async Task<SimpleTransaction> UpdateTransactionAsync(SimpleTransaction transaction)
    {
        await Task.Delay(100);
        
        var existingTransaction = _transactions.FirstOrDefault(t => t.Id == transaction.Id);
        if (existingTransaction != null)
        {
            existingTransaction.Description = transaction.Description;
            existingTransaction.Amount = transaction.Amount;
            existingTransaction.Date = transaction.Date;
            existingTransaction.Category = transaction.Category;
            existingTransaction.Type = transaction.Type;
            return existingTransaction;
        }
        
        throw new ArgumentException($"Transaction with ID {transaction.Id} not found");
    }

    public async Task<bool> DeleteTransactionAsync(int id)
    {
        await Task.Delay(100);
        
        var transaction = _transactions.FirstOrDefault(t => t.Id == id);
        if (transaction != null)
        {
            _transactions.Remove(transaction);
            return true;
        }
        
        return false;
    }
}