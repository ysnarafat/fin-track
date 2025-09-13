using FluentValidation;
using FinTrack.Core.Enums;
using FinTrack.Shared.DTOs;

namespace FinTrack.Shared.Validators;

/// <summary>
/// Validator for CreateTransactionDto
/// </summary>
public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transaction amount must be greater than 0");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Transaction description is required")
            .Length(1, 500).WithMessage("Description must be between 1 and 500 characters");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Transaction date is required")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1)).WithMessage("Transaction date cannot be more than 1 day in the future");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");

        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("Account is required");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid transaction type");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(50).WithMessage("Reference number cannot exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");

        // Transfer specific validation
        RuleFor(x => x.TransferToAccountId)
            .NotNull().WithMessage("Transfer destination account is required for transfer transactions")
            .GreaterThan(0).WithMessage("Transfer destination account must be valid")
            .When(x => x.Type == TransactionType.Transfer);

        RuleFor(x => x.TransferToAccountId)
            .NotEqual(x => x.AccountId).WithMessage("Cannot transfer to the same account")
            .When(x => x.Type == TransactionType.Transfer && x.TransferToAccountId.HasValue);

        RuleFor(x => x.TransferToAccountId)
            .Null().WithMessage("Transfer destination account should not be specified for non-transfer transactions")
            .When(x => x.Type != TransactionType.Transfer);
    }
}

/// <summary>
/// Validator for UpdateTransactionDto
/// </summary>
public class UpdateTransactionDtoValidator : AbstractValidator<UpdateTransactionDto>
{
    public UpdateTransactionDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Transaction ID must be greater than 0");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transaction amount must be greater than 0");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Transaction description is required")
            .Length(1, 500).WithMessage("Description must be between 1 and 500 characters");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Transaction date is required")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1)).WithMessage("Transaction date cannot be more than 1 day in the future");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");

        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("Account is required");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid transaction type");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(50).WithMessage("Reference number cannot exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");

        // Transfer specific validation
        RuleFor(x => x.TransferToAccountId)
            .NotNull().WithMessage("Transfer destination account is required for transfer transactions")
            .GreaterThan(0).WithMessage("Transfer destination account must be valid")
            .When(x => x.Type == TransactionType.Transfer);

        RuleFor(x => x.TransferToAccountId)
            .NotEqual(x => x.AccountId).WithMessage("Cannot transfer to the same account")
            .When(x => x.Type == TransactionType.Transfer && x.TransferToAccountId.HasValue);

        RuleFor(x => x.TransferToAccountId)
            .Null().WithMessage("Transfer destination account should not be specified for non-transfer transactions")
            .When(x => x.Type != TransactionType.Transfer);
    }
}