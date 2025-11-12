using FinTrack.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinTrack.Core.Entities;

/// <summary>
/// Represents a financial account in the system
/// </summary>
public class Account : BaseEntity
{
    /// <summary>
    /// Name of the account
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Current balance of the account
    /// </summary>
    [Required]
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Type of account (Checking, Savings, Credit Card, etc.)
    /// </summary>
    [Required]
    public AccountType Type { get; set; }
    
    /// <summary>
    /// Currency code for the account (e.g., USD, EUR)
    /// </summary>
    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Optional description of the account
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Account number or identifier
    /// </summary>
    [StringLength(50)]
    public string? AccountNumber { get; set; }
    
    /// <summary>
    /// Bank or institution name
    /// </summary>
    [StringLength(100)]
    public string? Institution { get; set; }
    
    /// <summary>
    /// Initial balance when the account was created
    /// </summary>
    public decimal InitialBalance { get; set; }
    
    /// <summary>
    /// Indicates if the account is currently active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Credit limit for credit card accounts
    /// </summary>
    public decimal? CreditLimit { get; set; }
    
    /// <summary>
    /// Interest rate for the account (if applicable)
    /// </summary>
    public decimal? InterestRate { get; set; }
    
    // Navigation properties
    
    /// <summary>
    /// Collection of transactions associated with this account
    /// </summary>
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>(); 
   
    /// <summary>
    /// Collection of transactions where this account is the transfer destination
    /// </summary>
    public virtual ICollection<Transaction> IncomingTransfers { get; set; } = new List<Transaction>();
    
    /// <summary>
    /// Collection of goals linked to this account
    /// </summary>
    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
    
    /// <summary>
    /// Constructor that initializes default values
    /// </summary>
    public Account()
    {
        Type = AccountType.Checking;
        Currency = "USD";
        IsActive = true;
        Balance = 0;
        InitialBalance = 0;
    }
    
    /// <summary>
    /// Gets the available balance (considering credit limit for credit cards)
    /// </summary>
    public decimal AvailableBalance
    {
        get
        {
            return Type == AccountType.CreditCard && CreditLimit.HasValue
                ? CreditLimit.Value + Balance // For credit cards, negative balance means debt
                : Balance;
        }
    }
    
    /// <summary>
    /// Gets the account balance as a formatted string
    /// </summary>
    public string FormattedBalance => $"{Balance:C}";
    
    /// <summary>
    /// Gets the available balance as a formatted string
    /// </summary>
    public string FormattedAvailableBalance => $"{AvailableBalance:C}";
    
    /// <summary>
    /// Indicates if the account is overdrawn or over credit limit
    /// </summary>
    public bool IsOverdrawn
    {
        get
        {
            return Type switch
            {
                AccountType.CreditCard => CreditLimit.HasValue && Balance < -CreditLimit.Value,
                _ => Balance < 0
            };
        }
    }
    
    /// <summary>
    /// Updates the account balance by adding the specified amount
    /// </summary>
    /// <param name="amount">Amount to add (can be negative for debits)</param>
    public void UpdateBalance(decimal amount)
    {
        Balance += amount;
        MarkAsModified();
    }
    
    /// <summary>
    /// Sets the account balance to a specific amount
    /// </summary>
    /// <param name="newBalance">New balance amount</param>
    public void SetBalance(decimal newBalance)
    {
        Balance = newBalance;
        MarkAsModified();
    }
    
    /// <summary>
    /// Validates the account data
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name)) return false;
        if (string.IsNullOrWhiteSpace(Currency) || Currency.Length != 3) return false;
        if (Type == AccountType.CreditCard && CreditLimit.HasValue && CreditLimit.Value < 0) return false;
        if (InterestRate.HasValue && InterestRate.Value < 0) return false;
        
        return true;
    }
    
    /// <summary>
    /// Calculates the total income for this account within a date range
    /// </summary>
    public decimal CalculateIncome(DateTime startDate, DateTime endDate)
    {
        return Transactions
            .Where(t => !t.IsDeleted && t.Date >= startDate && t.Date <= endDate && t.Type == TransactionType.Income)
            .Sum(t => t.Amount);
    }
    
    /// <summary>
    /// Calculates the total expenses for this account within a date range
    /// </summary>
    public decimal CalculateExpenses(DateTime startDate, DateTime endDate)
    {
        return Transactions
            .Where(t => !t.IsDeleted && t.Date >= startDate && t.Date <= endDate && t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);
    }
}