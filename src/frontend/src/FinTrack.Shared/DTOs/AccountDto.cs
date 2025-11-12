using FinTrack.Core.Enums;

namespace FinTrack.Shared.DTOs;

/// <summary>
/// Data transfer object for Account entity
/// </summary>
public class AccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public string? AccountNumber { get; set; }
    public string? Institution { get; set; }
    public decimal InitialBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? CreditLimit { get; set; }
    public decimal? InterestRate { get; set; }
}

/// <summary>
/// DTO for creating a new account
/// </summary>
public class CreateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public AccountType Type { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public string? AccountNumber { get; set; }
    public string? Institution { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? InterestRate { get; set; }
}

/// <summary>
/// DTO for updating an existing account
/// </summary>
public class UpdateAccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public string? AccountNumber { get; set; }
    public string? Institution { get; set; }
    public bool IsActive { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? InterestRate { get; set; }
}