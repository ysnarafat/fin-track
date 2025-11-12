using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using FinTrack.Shared.Services;
using FinTrack.Shared.Validators;
using FinTrack.Shared.DTOs;

namespace FinTrack.Shared.Extensions;

/// <summary>
/// Extension methods for registering validation services
/// </summary>
public static class ValidationServiceExtensions
{
    /// <summary>
    /// Registers all validation services and validators
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        // Register FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateAccountDtoValidator>();

        // Register validation services
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IBusinessRuleValidationService, BusinessRuleValidationService>();
        services.AddScoped<IValidationErrorService, ValidationErrorService>();
        services.AddScoped<IComprehensiveValidationService, ComprehensiveValidationService>();

        // Register specific validators
        services.AddScoped<IValidator<CreateAccountDto>, CreateAccountDtoValidator>();
        services.AddScoped<IValidator<UpdateAccountDto>, UpdateAccountDtoValidator>();
        services.AddScoped<IValidator<CreateTransactionDto>, CreateTransactionDtoValidator>();
        services.AddScoped<IValidator<UpdateTransactionDto>, UpdateTransactionDtoValidator>();
        services.AddScoped<IValidator<CreateCategoryDto>, CreateCategoryDtoValidator>();
        services.AddScoped<IValidator<UpdateCategoryDto>, UpdateCategoryDtoValidator>();
        services.AddScoped<IValidator<CreateBudgetDto>, CreateBudgetDtoValidator>();
        services.AddScoped<IValidator<UpdateBudgetDto>, UpdateBudgetDtoValidator>();
        services.AddScoped<IValidator<CreateGoalDto>, CreateGoalDtoValidator>();
        services.AddScoped<IValidator<UpdateGoalDto>, UpdateGoalDtoValidator>();

        return services;
    }

    /// <summary>
    /// Registers validation services with custom configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureValidation">Configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddValidationServices(this IServiceCollection services, Action<ValidationConfiguration> configureValidation)
    {
        var config = new ValidationConfiguration();
        configureValidation(config);

        // Register FluentValidation with configuration
        services.AddValidatorsFromAssemblyContaining<CreateAccountDtoValidator>(config.ServiceLifetime);

        // Register validation services
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IBusinessRuleValidationService, BusinessRuleValidationService>();
        services.AddScoped<IValidationErrorService, ValidationErrorService>();
        services.AddScoped<IComprehensiveValidationService, ComprehensiveValidationService>();

        // Configure FluentValidation options
        if (config.ValidatorOptions != null)
        {
            ValidatorOptions.Global.DefaultClassLevelCascadeMode = config.ValidatorOptions.DefaultClassLevelCascadeMode;
            ValidatorOptions.Global.DefaultRuleLevelCascadeMode = config.ValidatorOptions.DefaultRuleLevelCascadeMode;
        }

        return services;
    }
}

/// <summary>
/// Configuration options for validation services
/// </summary>
public class ValidationConfiguration
{
    /// <summary>
    /// Service lifetime for validators (default: Scoped)
    /// </summary>
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// FluentValidation options
    /// </summary>
    public ValidationOptions? ValidatorOptions { get; set; }
}

/// <summary>
/// FluentValidation configuration options
/// </summary>
public class ValidationOptions
{
    /// <summary>
    /// Default cascade mode for class-level validation
    /// </summary>
    public FluentValidation.CascadeMode DefaultClassLevelCascadeMode { get; set; } = FluentValidation.CascadeMode.Continue;

    /// <summary>
    /// Default cascade mode for rule-level validation
    /// </summary>
    public FluentValidation.CascadeMode DefaultRuleLevelCascadeMode { get; set; } = FluentValidation.CascadeMode.Stop;
}