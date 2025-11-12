using FluentValidation;
using FinTrack.Core.Enums;
using FinTrack.Shared.DTOs;

namespace FinTrack.Shared.Validators;

/// <summary>
/// Validator for CreateBudgetDto
/// </summary>
public class CreateBudgetDtoValidator : AbstractValidator<CreateBudgetDto>
{
    public CreateBudgetDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Budget name is required")
            .Length(1, 100).WithMessage("Budget name must be between 1 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Budget amount must be greater than 0");

        RuleFor(x => x.Period)
            .IsInEnum().WithMessage("Invalid budget period");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.AlertThreshold)
            .InclusiveBetween(0, 100).WithMessage("Alert threshold must be between 0 and 100")
            .When(x => x.AlertThreshold.HasValue);

        RuleFor(x => x.Color)
            .Must(BeValidHexColor).WithMessage("Color must be a valid hex color code (e.g., #FF0000)")
            .When(x => !string.IsNullOrEmpty(x.Color));

        // Period-specific date validation
        RuleFor(x => x)
            .Must(HaveValidPeriodDates).WithMessage("Date range does not match the selected budget period")
            .When(x => x.Period != BudgetPeriod.Custom);
    }

    private static bool BeValidHexColor(string? color)
    {
        if (string.IsNullOrEmpty(color)) return true;
        if (!color.StartsWith("#")) return false;
        if (color.Length != 7) return false;
        
        return color[1..].All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
    }

    private static bool HaveValidPeriodDates(CreateBudgetDto budget)
    {
        var expectedDays = budget.Period switch
        {
            BudgetPeriod.Weekly => 7,
            BudgetPeriod.Monthly => DateTime.DaysInMonth(budget.StartDate.Year, budget.StartDate.Month),
            BudgetPeriod.Quarterly => 90, // Approximate
            BudgetPeriod.Annual => DateTime.IsLeapYear(budget.StartDate.Year) ? 366 : 365,
            BudgetPeriod.Custom => -1, // Any range is valid for custom
            _ => -1
        };

        if (expectedDays == -1) return true; // Custom period or unknown

        var actualDays = (budget.EndDate - budget.StartDate).Days + 1;
        var tolerance = budget.Period == BudgetPeriod.Quarterly ? 5 : 2; // Allow some tolerance

        return Math.Abs(actualDays - expectedDays) <= tolerance;
    }
}

/// <summary>
/// Validator for UpdateBudgetDto
/// </summary>
public class UpdateBudgetDtoValidator : AbstractValidator<UpdateBudgetDto>
{
    public UpdateBudgetDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Budget ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Budget name is required")
            .Length(1, 100).WithMessage("Budget name must be between 1 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Budget amount must be greater than 0");

        RuleFor(x => x.Period)
            .IsInEnum().WithMessage("Invalid budget period");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.AlertThreshold)
            .InclusiveBetween(0, 100).WithMessage("Alert threshold must be between 0 and 100")
            .When(x => x.AlertThreshold.HasValue);

        RuleFor(x => x.Color)
            .Must(BeValidHexColor).WithMessage("Color must be a valid hex color code (e.g., #FF0000)")
            .When(x => !string.IsNullOrEmpty(x.Color));

        // Period-specific date validation
        RuleFor(x => x)
            .Must(HaveValidPeriodDates).WithMessage("Date range does not match the selected budget period")
            .When(x => x.Period != BudgetPeriod.Custom);
    }

    private static bool BeValidHexColor(string? color)
    {
        if (string.IsNullOrEmpty(color)) return true;
        if (!color.StartsWith("#")) return false;
        if (color.Length != 7) return false;
        
        return color[1..].All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
    }

    private static bool HaveValidPeriodDates(UpdateBudgetDto budget)
    {
        var expectedDays = budget.Period switch
        {
            BudgetPeriod.Weekly => 7,
            BudgetPeriod.Monthly => DateTime.DaysInMonth(budget.StartDate.Year, budget.StartDate.Month),
            BudgetPeriod.Quarterly => 90, // Approximate
            BudgetPeriod.Annual => DateTime.IsLeapYear(budget.StartDate.Year) ? 366 : 365,
            BudgetPeriod.Custom => -1, // Any range is valid for custom
            _ => -1
        };

        if (expectedDays == -1) return true; // Custom period or unknown

        var actualDays = (budget.EndDate - budget.StartDate).Days + 1;
        var tolerance = budget.Period == BudgetPeriod.Quarterly ? 5 : 2; // Allow some tolerance

        return Math.Abs(actualDays - expectedDays) <= tolerance;
    }
}