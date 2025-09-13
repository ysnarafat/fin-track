using FinTrack.Core.Enums;

namespace FinTrack.Shared.DTOs;

/// <summary>
/// Data transfer object for Transaction entity
/// </summary>
public class TransactionDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public int AccountId { get; set; }
    public TransactionType Type { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public int? TransferToAccountId { get; set; }
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledAt { get; set; }
    
    // Navigation properties for display
    public string? CategoryName { get; set; }
    public string? AccountName { get; set; }
    public string? TransferToAccountName { get; set; }
}

/// <summary>
/// DTO for creating a new transaction
/// </summary>
public class CreateTransactionDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public int CategoryId { get; set; }
    public int AccountId { get; set; }
    public TransactionType Type { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public int? TransferToAccountId { get; set; }
}

/// <summary>
/// DTO for updating an existing transaction
/// </summary>
public class UpdateTransactionDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public int AccountId { get; set; }
    public TransactionType Type { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public int? TransferToAccountId { get; set; }
    public bool IsReconciled { get; set; }
}