using FluentValidation;
using FinTrack.Core.Enums;
using FinTrack.Shared.DTOs;

namespace FinTrack.Shared.Validators;

/// <summary>
/// Validator for CreateGoalDto
/// </summary>
public class CreateGoalDtoValidator : AbstractValidator<CreateGoalDto>
{
    public CreateGoalDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Goal name is required")
            .Length(1, 100).WithMessage("Goal name must be between 1 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Target amount must be greater than 0");

        RuleFor(x => x.TargetDate)
            .NotEmpty().WithMessage("Target date is required")
            .GreaterThan(DateTime.Today).WithMessage("Target date must be in the future");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5).WithMessage("Priority must be between 1 (highest) and 5 (lowest)");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid goal type");

        RuleFor(x => x.Color)
            .Must(BeValidHexColor).WithMessage("Color must be a valid hex color code (e.g., #FF0000)")
            .When(x => !string.IsNullOrEmpty(x.Color));

        RuleFor(x => x.LinkedAccountId)
            .GreaterThan(0).WithMessage("Linked account ID must be greater than 0")
            .When(x => x.LinkedAccountId.HasValue);

        // Goal type specific validation
        RuleFor(x => x.TargetDate)
            .LessThanOrEqualTo(DateTime.Today.AddYears(50)).WithMessage("Target date cannot be more than 50 years in the future")
            .When(x => x.Type == GoalType.Retirement);

        RuleFor(x => x.TargetDate)
            .LessThanOrEqualTo(DateTime.Today.AddYears(10)).WithMessage("Target date should be within 10 years for most goals")
            .When(x => x.Type != GoalType.Retirement);
    }

    private static bool BeValidHexColor(string? color)
    {
        if (string.IsNullOrEmpty(color)) return true;
        if (!color.StartsWith("#")) return false;
        if (color.Length != 7) return false;
        
        return color[1..].All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
    }
}

/// <summary>
/// Validator for UpdateGoalDto
/// </summary>
public class UpdateGoalDtoValidator : AbstractValidator<UpdateGoalDto>
{
    public UpdateGoalDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Goal ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Goal name is required")
            .Length(1, 100).WithMessage("Goal name must be between 1 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Target amount must be greater than 0");

        RuleFor(x => x.CurrentAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Current amount cannot be negative");

        RuleFor(x => x.TargetDate)
            .NotEmpty().WithMessage("Target date is required")
            .GreaterThan(DateTime.Today).WithMessage("Target date must be in the future");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5).WithMessage("Priority must be between 1 (highest) and 5 (lowest)");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid goal type");

        RuleFor(x => x.Color)
            .Must(BeValidHexColor).WithMessage("Color must be a valid hex color code (e.g., #FF0000)")
            .When(x => !string.IsNullOrEmpty(x.Color));

        RuleFor(x => x.LinkedAccountId)
            .GreaterThan(0).WithMessage("Linked account ID must be greater than 0")
            .When(x => x.LinkedAccountId.HasValue);

        // Logical validation
        RuleFor(x => x.CurrentAmount)
            .LessThanOrEqualTo(x => x.TargetAmount * 1.1m).WithMessage("Current amount should not exceed target amount by more than 10%");

        // Goal type specific validation
        RuleFor(x => x.TargetDate)
            .LessThanOrEqualTo(DateTime.Today.AddYears(50)).WithMessage("Target date cannot be more than 50 years in the future")
            .When(x => x.Type == GoalType.Retirement);

        RuleFor(x => x.TargetDate)
            .LessThanOrEqualTo(DateTime.Today.AddYears(10)).WithMessage("Target date should be within 10 years for most goals")
            .When(x => x.Type != GoalType.Retirement);
    }

    private static bool BeValidHexColor(string? color)
    {
        if (string.IsNullOrEmpty(color)) return true;
        if (!color.StartsWith("#")) return false;
        if (color.Length != 7) return false;
        
        return color[1..].All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
    }
}