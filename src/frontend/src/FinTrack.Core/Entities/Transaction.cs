using FinTrack.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinTrack.Core.Entities;

/// <summary>
/// Represents a financial transaction in the system
/// </summary>
public class Transaction : BaseEntity
{
    /// <summary>
    /// The monetary amount of the transaction
    /// </summary>
    [Required]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Description or memo for the transaction
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Date when the transaction occurred
    /// </summary>
    [Required]
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Foreign key to the category this transaction belongs to
    /// </summary>
    [Required]
    public int CategoryId { get; set; }
    
    /// <summary>
    /// Foreign key to the account this transaction belongs to
    /// </summary>
    [Required]
    public int AccountId { get; set; }
    
    /// <summary>
    /// Type of transaction (Income, Expense, Transfer)
    /// </summary>
    [Required]
    public TransactionType Type { get; set; }
    
    /// <summary>
    /// Optional reference number or check number
    /// </summary>
    [StringLength(50)]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Optional notes or additional details
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// For transfer transactions, the ID of the destination account
    /// </summary>
    public int? TransferToAccountId { get; set; }
    
    /// <summary>
    /// For transfer transactions, the ID of the linked transaction in the destination account
    /// </summary>
    public int? LinkedTransactionId { get; set; }
    
    /// <summary>
    /// Indicates if this transaction has been reconciled
    /// </summary>
    public bool IsReconciled { get; set; }
    
    /// <summary>
    /// Date when the transaction was reconciled
    /// </summary>
    public DateTime? ReconciledAt { get; set; }
    
    // Navigation properties
    
    /// <summary>
    /// Navigation property to the category this transaction belongs to
    /// </summary>
    public virtual Category? Category { get; set; }
    
    /// <summary>
    /// Navigation property to the account this transaction belongs to
    /// </summary>
    public virtual Account? Account { get; set; }
    
    /// <summary>
    /// Navigation property to the destination account for transfer transactions
    /// </summary>
    public virtual Account? TransferToAccount { get; set; }
    
    /// <summary>
    /// Navigation property to the linked transaction for transfers
    /// </summary>
    public virtual Transaction? LinkedTransaction { get; set; }
    
    /// <summary>
    /// Constructor that initializes default values
    /// </summary>
    public Transaction()
    {
        Date = DateTime.Today;
        Type = TransactionType.Expense;
        IsReconciled = false;
    }
    
    /// <summary>
    /// Gets the absolute amount (always positive)
    /// </summary>
    public decimal AbsoluteAmount => Math.Abs(Amount);
    
    /// <summary>
    /// Gets the signed amount based on transaction type
    /// </summary>
    public decimal SignedAmount => Type switch
    {
        TransactionType.Income => Math.Abs(Amount),
        TransactionType.Expense => -Math.Abs(Amount),
        TransactionType.Transfer => Amount, // Transfer amounts can be positive or negative
        _ => Amount
    };
    
    /// <summary>
    /// Validates the transaction data
    /// </summary>
    public bool IsValid()
    {
        if (Amount <= 0) return false;
        if (string.IsNullOrWhiteSpace(Description)) return false;
        if (CategoryId <= 0) return false;
        if (AccountId <= 0) return false;
        if (Date > DateTime.Today.AddDays(1)) return false; // Allow future dates up to tomorrow
        
        // Transfer-specific validation
        if (Type == TransactionType.Transfer)
        {
            if (TransferToAccountId == null || TransferToAccountId <= 0) return false;
            if (TransferToAccountId == AccountId) return false; // Can't transfer to same account
        }
        
        return true;
    }
    
    /// <summary>
    /// Creates a copy of this transaction for transfer purposes
    /// </summary>
    public Transaction CreateTransferCounterpart(int destinationAccountId)
    {
        if (Type != TransactionType.Transfer)
            throw new InvalidOperationException("Can only create counterpart for transfer transactions");
            
        return new Transaction
        {
            Amount = Amount,
            Description = $"Transfer from {Account?.Name ?? "Account"}",
            Date = Date,
            CategoryId = CategoryId,
            AccountId = destinationAccountId,
            Type = TransactionType.Transfer,
            ReferenceNumber = ReferenceNumber,
            Notes = Notes,
            TransferToAccountId = AccountId,
            LinkedTransactionId = Id,
            IsReconciled = IsReconciled,
            ReconciledAt = ReconciledAt
        };
    }
}