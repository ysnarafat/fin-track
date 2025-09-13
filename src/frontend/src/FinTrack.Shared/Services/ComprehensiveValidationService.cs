using FluentValidation;
using FluentValidation.Results;
using FinTrack.Core.Exceptions;
using FinTrack.Shared.DTOs;

namespace FinTrack.Shared.Services;

/// <summary>
/// Comprehensive validation service that combines FluentValidation and business rule validation
/// </summary>
public interface IComprehensiveValidationService
{
    Task<ValidationResultWrapper> ValidateAsync<T>(T dto, CancellationToken cancellationToken = default);
    Task<ValidationResultWrapper> ValidateCreateAccountAsync(CreateAccountDto dto, CancellationToken cancellationToken = default);
    Task<ValidationResultWrapper> ValidateUpdateAccountAsync(UpdateAccountDto dto, CancellationToken cancellationToken = default);
    Task<ValidationResultWrapper> ValidateCreateTransactionAsync(CreateTransactionDto dto, CancellationToken cancellationToken = default);
    Task<ValidationResultWrapper> ValidateUpdateTransactionAsync(UpdateTransactionDto dto, CancellationToken cancellationToken = default);
    Task<ValidationResultWrapper> ValidateCreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<ValidationResultWrapper> ValidateUpdateCategoryAsync(UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<ValidationResultWrapper> ValidateCreateBudgetAsync(CreateBudgetDto dto, CancellationToken cancellationToken = default);
    Task<ValidationResultWrapper> ValidateUpdateBudgetAsync(UpdateBudgetDto dto, CancellationToken cancellationToken = default);
    Task<ValidationResultWrapper> ValidateCreateGoalAsync(CreateGoalDto dto, CancellationToken cancellationToken = default);
    Task<ValidationResultWrapper> ValidateUpdateGoalAsync(UpdateGoalDto dto, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of comprehensive validation service
/// </summary>
public class ComprehensiveValidationService : IComprehensiveValidationService
{
    private readonly IValidationService _validationService;
    private readonly IBusinessRuleValidationService _businessRuleValidationService;
    private readonly IValidationErrorService _validationErrorService;

    public ComprehensiveValidationService(
        IValidationService validationService,
        IBusinessRuleValidationService businessRuleValidationService,
        IValidationErrorService validationErrorService)
    {
        _validationService = validationService;
        _businessRuleValidationService = businessRuleValidationService;
        _validationErrorService = validationErrorService;
    }

    public async Task<ValidationResultWrapper> ValidateAsync<T>(T dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = await _validationService.ValidateAsync(dto, cancellationToken);
            return ValidationResultWrapper.FromValidationResult(validationResult, _validationErrorService);
        }
        catch (Exception ex)
        {
            return ValidationResultWrapper.FromException(ex, _validationErrorService);
        }
    }

    public async Task<ValidationResultWrapper> ValidateCreateAccountAsync(CreateAccountDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // First, run FluentValidation
            var validationResult = await _validationService.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return ValidationResultWrapper.FromValidationResult(validationResult, _validationErrorService);
            }

            // Then, run business rule validation
            await _businessRuleValidationService.ValidateAccountBusinessRulesAsync(dto, cancellationToken);

            return ValidationResultWrapper.Success();
        }
        catch (Exception ex)
        {
            return ValidationResultWrapper.FromException(ex, _validationErrorService);
        }
    }

    public async Task<ValidationResultWrapper> ValidateUpdateAccountAsync(UpdateAccountDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // First, run FluentValidation
            var validationResult = await _validationService.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return ValidationResultWrapper.FromValidationResult(validationResult, _validationErrorService);
            }

            // Then, run business rule validation
            await _businessRuleValidationService.ValidateAccountBusinessRulesAsync(dto, cancellationToken);

            return ValidationResultWrapper.Success();
        }
        catch (Exception ex)
        {
            return ValidationResultWrapper.FromException(ex, _validationErrorService);
        }
    }

    public async Task<ValidationResultWrapper> ValidateCreateTransactionAsync(CreateTransactionDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // First, run FluentValidation
            var validationResult = await _validationService.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return ValidationResultWrapper.FromValidationResult(validationResult, _validationErrorService);
            }

            // Then, run business rule validation
            await _businessRuleValidationService.ValidateTransactionBusinessRulesAsync(dto, cancellationToken);

            return ValidationResultWrapper.Success();
        }
        catch (Exception ex)
        {
            return ValidationResultWrapper.FromException(ex, _validationErrorService);
        }
    }

    public async Task<ValidationResultWrapper> ValidateUpdateTransactionAsync(UpdateTransactionDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // First, run FluentValidation
            var validationResult = await _validationService.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return ValidationResultWrapper.FromValidationResult(validationResult, _validationErrorService);
            }

            // Then, run business rule validation
            await _businessRuleValidationService.ValidateTransactionBusinessRulesAsync(dto, cancellationToken);

            return ValidationResultWrapper.Success();
        }
        catch (Exception ex)
        {
            return ValidationResultWrapper.FromException(ex, _validationErrorService);
        }
    }

    public async Task<ValidationResultWrapper> ValidateCreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // First, run FluentValidation
            var validationResult = await _validationService.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return ValidationResultWrapper.FromValidationResult(validationResult, _validationErrorService);
            }

            // Then, run business rule validation
            await _businessRuleValidationService.ValidateCategoryBusinessRulesAsync(dto, cancellationToken);

            return ValidationResultWrapper.Success();
        }
        catch (Exception ex)
        {
            return ValidationResultWrapper.FromException(ex, _validationErrorService);
        }
    }

    public async Task<ValidationResultWrapper> ValidateUpdateCategoryAsync(UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // First, run FluentValidation
            var validationResult = await _validationService.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return ValidationResultWrapper.FromValidationResult(validationResult, _validationErrorService);
            }

            // Then, run business rule validation
            await _businessRuleValidationService.ValidateCategoryBusinessRulesAsync(dto, cancellationToken);

            return ValidationResultWrapper.Success();
        }
        catch (Exception ex)
        {
            return ValidationResultWrapper.FromException(ex, _validationErrorService);
        }
    }

    public async Task<ValidationResultWrapper> ValidateCreateBudgetAsync(CreateBudgetDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // First, run FluentValidation
            var validationResult = await _validationService.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return ValidationResultWrapper.FromValidationResult(validationResult, _validationErrorService);
            }

            // Then, run business rule validation
            await _businessRuleValidationService.ValidateBudgetBusinessRulesAsync(dto, cancellationToken);

            return ValidationResultWrapper.Success();
        }
        catch (Exception ex)
        {
            return ValidationResultWrapper.FromException(ex, _validationErrorService);
        }
    }

    public async Task<ValidationResultWrapper> ValidateUpdateBudgetAsync(UpdateBudgetDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // First, run FluentValidation
            var validationResult = await _validationService.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return ValidationResultWrapper.FromValidationResult(validationResult, _validationErrorService);
            }

            // Then, run business rule validation
            await _businessRuleValidationService.ValidateBudgetBusinessRulesAsync(dto, cancellationToken);

            return ValidationResultWrapper.Success();
        }
        catch (Exception ex)
        {
            return ValidationResultWrapper.FromException(ex, _validationErrorService);
        }
    }

    public async Task<ValidationResultWrapper> ValidateCreateGoalAsync(CreateGoalDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // First, run FluentValidation
            var validationResult = await _validationService.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return ValidationResultWrapper.FromValidationResult(validationResult, _validationErrorService);
            }

            // Then, run business rule validation
            await _businessRuleValidationService.ValidateGoalBusinessRulesAsync(dto, cancellationToken);

            return ValidationResultWrapper.Success();
        }
        catch (Exception ex)
        {
            return ValidationResultWrapper.FromException(ex, _validationErrorService);
        }
    }

    public async Task<ValidationResultWrapper> ValidateUpdateGoalAsync(UpdateGoalDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // First, run FluentValidation
            var validationResult = await _validationService.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return ValidationResultWrapper.FromValidationResult(validationResult, _validationErrorService);
            }

            // Then, run business rule validation
            await _businessRuleValidationService.ValidateGoalBusinessRulesAsync(dto, cancellationToken);

            return ValidationResultWrapper.Success();
        }
        catch (Exception ex)
        {
            return ValidationResultWrapper.FromException(ex, _validationErrorService);
        }
    }
}

