using FluentValidation;
using FinTrack.Core.Enums;
using FinTrack.Shared.DTOs;

namespace FinTrack.Shared.Validators;

/// <summary>
/// Validator for CreateCategoryDto
/// </summary>
public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .Length(1, 100).WithMessage("Category name must be between 1 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Color)
            .Must(BeValidHexColor).WithMessage("Color must be a valid hex color code (e.g., #FF0000)")
            .When(x => !string.IsNullOrEmpty(x.Color));

        RuleFor(x => x.Icon)
            .MaximumLength(50).WithMessage("Icon name cannot exceed 50 characters");

        RuleFor(x => x.ParentCategoryId)
            .GreaterThan(0).WithMessage("Parent category ID must be greater than 0")
            .When(x => x.ParentCategoryId.HasValue);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative");

        RuleFor(x => x.BudgetLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Budget limit cannot be negative")
            .When(x => x.BudgetLimit.HasValue);

        RuleFor(x => x.CategoryType)
            .IsInEnum().WithMessage("Invalid category type");
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
/// Validator for UpdateCategoryDto
/// </summary>
public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .Length(1, 100).WithMessage("Category name must be between 1 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Color)
            .Must(BeValidHexColor).WithMessage("Color must be a valid hex color code (e.g., #FF0000)")
            .When(x => !string.IsNullOrEmpty(x.Color));

        RuleFor(x => x.Icon)
            .MaximumLength(50).WithMessage("Icon name cannot exceed 50 characters");

        RuleFor(x => x.ParentCategoryId)
            .GreaterThan(0).WithMessage("Parent category ID must be greater than 0")
            .NotEqual(x => x.Id).WithMessage("Category cannot be its own parent")
            .When(x => x.ParentCategoryId.HasValue);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative");

        RuleFor(x => x.BudgetLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Budget limit cannot be negative")
            .When(x => x.BudgetLimit.HasValue);

        RuleFor(x => x.CategoryType)
            .IsInEnum().WithMessage("Invalid category type");
    }

    private static bool BeValidHexColor(string? color)
    {
        if (string.IsNullOrEmpty(color)) return true;
        if (!color.StartsWith("#")) return false;
        if (color.Length != 7) return false;
        
        return color[1..].All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
    }
}