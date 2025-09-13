using FluentValidation;
using FinTrack.Core.Enums;
using FinTrack.Shared.DTOs;

namespace FinTrack.Shared.Validators;

/// <summary>
/// Validator for CreateAccountDto
/// </summary>
public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    public CreateAccountDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required")
            .Length(1, 100).WithMessage("Account name must be between 1 and 100 characters");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3, 3).WithMessage("Currency must be exactly 3 characters")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be a valid 3-letter code (e.g., USD, EUR)");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid account type");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.AccountNumber)
            .MaximumLength(50).WithMessage("Account number cannot exceed 50 characters");

        RuleFor(x => x.Institution)
            .MaximumLength(100).WithMessage("Institution name cannot exceed 100 characters");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit cannot be negative")
            .When(x => x.CreditLimit.HasValue);

        RuleFor(x => x.InterestRate)
            .GreaterThanOrEqualTo(0).WithMessage("Interest rate cannot be negative")
            .LessThanOrEqualTo(100).WithMessage("Interest rate cannot exceed 100%")
            .When(x => x.InterestRate.HasValue);

        // Credit card specific validation
        RuleFor(x => x.CreditLimit)
            .NotNull().WithMessage("Credit limit is required for credit card accounts")
            .GreaterThan(0).WithMessage("Credit limit must be greater than 0 for credit card accounts")
            .When(x => x.Type == AccountType.CreditCard);
    }
}

/// <summary>
/// Validator for UpdateAccountDto
/// </summary>
public class UpdateAccountDtoValidator : AbstractValidator<UpdateAccountDto>
{
    public UpdateAccountDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Account ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required")
            .Length(1, 100).WithMessage("Account name must be between 1 and 100 characters");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3, 3).WithMessage("Currency must be exactly 3 characters")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be a valid 3-letter code (e.g., USD, EUR)");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid account type");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.AccountNumber)
            .MaximumLength(50).WithMessage("Account number cannot exceed 50 characters");

        RuleFor(x => x.Institution)
            .MaximumLength(100).WithMessage("Institution name cannot exceed 100 characters");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit cannot be negative")
            .When(x => x.CreditLimit.HasValue);

        RuleFor(x => x.InterestRate)
            .GreaterThanOrEqualTo(0).WithMessage("Interest rate cannot be negative")
            .LessThanOrEqualTo(100).WithMessage("Interest rate cannot exceed 100%")
            .When(x => x.InterestRate.HasValue);

        // Credit card specific validation
        RuleFor(x => x.CreditLimit)
            .NotNull().WithMessage("Credit limit is required for credit card accounts")
            .GreaterThan(0).WithMessage("Credit limit must be greater than 0 for credit card accounts")
            .When(x => x.Type == AccountType.CreditCard);
    }
}