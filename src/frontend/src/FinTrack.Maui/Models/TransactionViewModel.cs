using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Maui.Models;

public class TransactionViewModel
{
    private readonly Transaction _transaction;

    public TransactionViewModel(Transaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    public int Id => _transaction.Id;
    public decimal Amount => _transaction.Amount;
    public string Description => _transaction.Description ?? "Unknown";
    public DateTime Date => _transaction.Date;
    public TransactionType Type => _transaction.Type;
    public string AccountName => _transaction.Account?.Name ?? "Unknown Account";
    public string CategoryName => _transaction.Category?.Name ?? "Uncategorized";
    public string CategoryColor => _transaction.Category?.Color ?? "#6B7280";
    public bool IsReconciled => _transaction.IsReconciled;
    public string? ReferenceNumber => _transaction.ReferenceNumber;
    public string? Notes => _transaction.Notes;

    public string FormattedDate => Date.ToString("MMM dd, yyyy");

    public string FormattedAmount
    {
        get
        {
            var prefix = Type switch
            {
                TransactionType.Income => "+",
                TransactionType.Expense => "-",
                TransactionType.Transfer => "",
                _ => ""
            };
            return $"{prefix}{Amount:C}";
        }
    }

    public string AmountColor => Type switch
    {
        TransactionType.Income => "#4CAF50",
        TransactionType.Expense => "#F44336",
        TransactionType.Transfer => "#2196F3",
        _ => "#000000"
    };

    public string TypeColor => Type switch
    {
        TransactionType.Income => "#4CAF50",
        TransactionType.Expense => "#F44336",
        TransactionType.Transfer => "#2196F3",
        _ => "#6B7280"
    };

    public string TypeIcon => Type switch
    {
        TransactionType.Income => "↗️",
        TransactionType.Expense => "↙️",
        TransactionType.Transfer => "↔️",
        _ => "💰"
    };

    public Transaction GetTransaction() => _transaction;
}