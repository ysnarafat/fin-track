using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Exceptions;
using FinTrack.Core.Interfaces;
using FinTrack.Shared.DTOs;

namespace FinTrack.Shared.Services;

/// <summary>
/// Service for validating business rules that require database access
/// </summary>
public interface IBusinessRuleValidationService
{
    Task ValidateAccountBusinessRulesAsync(CreateAccountDto dto, CancellationToken cancellationToken = default);
    Task ValidateAccountBusinessRulesAsync(UpdateAccountDto dto, CancellationToken cancellationToken = default);
    Task ValidateTransactionBusinessRulesAsync(CreateTransactionDto dto, CancellationToken cancellationToken = default);
    Task ValidateTransactionBusinessRulesAsync(UpdateTransactionDto dto, CancellationToken cancellationToken = default);
    Task ValidateCategoryBusinessRulesAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task ValidateCategoryBusinessRulesAsync(UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task ValidateBudgetBusinessRulesAsync(CreateBudgetDto dto, CancellationToken cancellationToken = default);
    Task ValidateBudgetBusinessRulesAsync(UpdateBudgetDto dto, CancellationToken cancellationToken = default);
    Task ValidateGoalBusinessRulesAsync(CreateGoalDto dto, CancellationToken cancellationToken = default);
    Task ValidateGoalBusinessRulesAsync(UpdateGoalDto dto, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of business rule validation service
/// </summary>
public class BusinessRuleValidationService : IBusinessRuleValidationService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IGoalRepository _goalRepository;

    public BusinessRuleValidationService(
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        IBudgetRepository budgetRepository,
        IGoalRepository goalRepository)
    {
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _budgetRepository = budgetRepository;
        _goalRepository = goalRepository;
    }

    public async Task ValidateAccountBusinessRulesAsync(CreateAccountDto dto, CancellationToken cancellationToken = default)
    {
        // Check for duplicate account names
        var existingAccount = await _accountRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (existingAccount != null)
        {
            throw new BusinessRuleException("DuplicateAccountName", $"An account with the name '{dto.Name}' already exists.");
        }

        // Validate currency code (basic validation - in real app, you might check against a currency service)
        if (!IsValidCurrencyCode(dto.Currency))
        {
            throw new BusinessRuleException("ValidationError", $"'{dto.Currency}' is not a valid currency code.");
        }
    }

    public async Task ValidateAccountBusinessRulesAsync(UpdateAccountDto dto, CancellationToken cancellationToken = default)
    {
        // Check if account exists
        var existingAccount = await _accountRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (existingAccount == null)
        {
            throw new BusinessRuleException("ValidationError", $"Account with ID {dto.Id} does not exist.");
        }

        // Check for duplicate account names (excluding current account)
        var duplicateAccount = await _accountRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (duplicateAccount != null && duplicateAccount.Id != dto.Id)
        {
            throw new BusinessRuleException("ValidationError", $"An account with the name '{dto.Name}' already exists.");
        }

        // Validate currency code
        if (!IsValidCurrencyCode(dto.Currency))
        {
            throw new BusinessRuleException("ValidationError", $"'{dto.Currency}' is not a valid currency code.");
        }
    }

    public async Task ValidateTransactionBusinessRulesAsync(CreateTransactionDto dto, CancellationToken cancellationToken = default)
    {
        // Validate account exists
        var account = await _accountRepository.GetByIdAsync(dto.AccountId, cancellationToken);
        if (account == null)
        {
            throw new BusinessRuleException("ValidationError", $"Account with ID {dto.AccountId} does not exist.");
        }

        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new BusinessRuleException("ValidationError", $"Category with ID {dto.CategoryId} does not exist.");
        }

        // Validate category is active
        if (!category.IsActive)
        {
            throw new BusinessRuleException("ValidationError", $"Cannot create transaction with inactive category '{category.Name}'.");
        }

        // Validate category type matches transaction type
        if (category.CategoryType != dto.Type && dto.Type != TransactionType.Transfer)
        {
            throw new BusinessRuleException("ValidationError", $"Category '{category.Name}' is for {category.CategoryType} transactions, but transaction type is {dto.Type}.");
        }

        // Validate transfer-specific rules
        if (dto.Type == TransactionType.Transfer && dto.TransferToAccountId.HasValue)
        {
            var transferToAccount = await _accountRepository.GetByIdAsync(dto.TransferToAccountId.Value, cancellationToken);
            if (transferToAccount == null)
            {
                throw new BusinessRuleException("ValidationError", $"Transfer destination account with ID {dto.TransferToAccountId.Value} does not exist.");
            }

            if (!transferToAccount.IsActive)
            {
                throw new BusinessRuleException("ValidationError", $"Cannot transfer to inactive account '{transferToAccount.Name}'.");
            }
        }

        // Validate account is active
        if (!account.IsActive)
        {
            throw new BusinessRuleException("ValidationError", $"Cannot create transaction for inactive account '{account.Name}'.");
        }
    }

    public async Task ValidateTransactionBusinessRulesAsync(UpdateTransactionDto dto, CancellationToken cancellationToken = default)
    {
        // All the same validations as create, plus existence check
        var existingTransaction = await _accountRepository.GetTransactionByIdAsync(dto.Id, cancellationToken);
        if (existingTransaction == null)
        {
            throw new BusinessRuleException("ValidationError", $"Transaction with ID {dto.Id} does not exist.");
        }

        // Validate account exists
        var account = await _accountRepository.GetByIdAsync(dto.AccountId, cancellationToken);
        if (account == null)
        {
            throw new BusinessRuleException("ValidationError", $"Account with ID {dto.AccountId} does not exist.");
        }

        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new BusinessRuleException("ValidationError", $"Category with ID {dto.CategoryId} does not exist.");
        }

        // Validate category is active
        if (!category.IsActive)
        {
            throw new BusinessRuleException("ValidationError", $"Cannot update transaction with inactive category '{category.Name}'.");
        }

        // Validate category type matches transaction type
        if (category.CategoryType != dto.Type && dto.Type != TransactionType.Transfer)
        {
            throw new BusinessRuleException("ValidationError", $"Category '{category.Name}' is for {category.CategoryType} transactions, but transaction type is {dto.Type}.");
        }

        // Validate transfer-specific rules
        if (dto.Type == TransactionType.Transfer && dto.TransferToAccountId.HasValue)
        {
            var transferToAccount = await _accountRepository.GetByIdAsync(dto.TransferToAccountId.Value, cancellationToken);
            if (transferToAccount == null)
            {
                throw new BusinessRuleException("ValidationError", $"Transfer destination account with ID {dto.TransferToAccountId.Value} does not exist.");
            }

            if (!transferToAccount.IsActive)
            {
                throw new BusinessRuleException("ValidationError", $"Cannot transfer to inactive account '{transferToAccount.Name}'.");
            }
        }

        // Validate account is active
        if (!account.IsActive)
        {
            throw new BusinessRuleException("ValidationError", $"Cannot update transaction for inactive account '{account.Name}'.");
        }
    }

    public async Task ValidateCategoryBusinessRulesAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        // Check for duplicate category names
        var existingCategory = await _categoryRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (existingCategory != null)
        {
            throw new BusinessRuleException("ValidationError", $"A category with the name '{dto.Name}' already exists.");
        }

        // Validate parent category exists and is active
        if (dto.ParentCategoryId.HasValue)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(dto.ParentCategoryId.Value, cancellationToken);
            if (parentCategory == null)
            {
                throw new BusinessRuleException("ValidationError", $"Parent category with ID {dto.ParentCategoryId.Value} does not exist.");
            }

            if (!parentCategory.IsActive)
            {
                throw new BusinessRuleException("ValidationError", $"Cannot create subcategory under inactive parent category '{parentCategory.Name}'.");
            }

            // Validate category type matches parent
            if (parentCategory.CategoryType != dto.CategoryType)
            {
                throw new BusinessRuleException("ValidationError", $"Subcategory type must match parent category type ({parentCategory.CategoryType}).");
            }
        }
    }

    public async Task ValidateCategoryBusinessRulesAsync(UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        // Check if category exists
        var existingCategory = await _categoryRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (existingCategory == null)
        {
            throw new BusinessRuleException("ValidationError", $"Category with ID {dto.Id} does not exist.");
        }

        // Check for duplicate category names (excluding current category)
        var duplicateCategory = await _categoryRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (duplicateCategory != null && duplicateCategory.Id != dto.Id)
        {
            throw new BusinessRuleException("ValidationError", $"A category with the name '{dto.Name}' already exists.");
        }

        // Validate parent category exists and is active
        if (dto.ParentCategoryId.HasValue)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(dto.ParentCategoryId.Value, cancellationToken);
            if (parentCategory == null)
            {
                throw new BusinessRuleException("ValidationError", $"Parent category with ID {dto.ParentCategoryId.Value} does not exist.");
            }

            if (!parentCategory.IsActive)
            {
                throw new BusinessRuleException("ValidationError", $"Cannot set inactive parent category '{parentCategory.Name}'.");
            }

            // Validate category type matches parent
            if (parentCategory.CategoryType != dto.CategoryType)
            {
                throw new BusinessRuleException("ValidationError", $"Category type must match parent category type ({parentCategory.CategoryType}).");
            }

            // Prevent circular references
            if (await WouldCreateCircularReference(dto.Id, dto.ParentCategoryId.Value, cancellationToken))
            {
                throw new BusinessRuleException("ValidationError", "Cannot set parent category as it would create a circular reference.");
            }
        }

        // Validate system categories cannot be deleted or have certain properties changed
        if (existingCategory.IsSystem)
        {
            if (!dto.IsActive)
            {
                throw new BusinessRuleException("ValidationError", "System categories cannot be deactivated.");
            }
        }
    }

    public async Task ValidateBudgetBusinessRulesAsync(CreateBudgetDto dto, CancellationToken cancellationToken = default)
    {
        // Validate category exists if specified
        if (dto.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId.Value, cancellationToken);
            if (category == null)
            {
                throw new BusinessRuleException("ValidationError", $"Category with ID {dto.CategoryId.Value} does not exist.");
            }

            if (!category.IsActive)
            {
                throw new BusinessRuleException("ValidationError", $"Cannot create budget for inactive category '{category.Name}'.");
            }
        }

        // Check for overlapping budgets for the same category
        var overlappingBudgets = await _budgetRepository.GetOverlappingBudgetsAsync(
            dto.CategoryId, dto.StartDate, dto.EndDate, cancellationToken);

        if (overlappingBudgets.Any())
        {
            var categoryName = dto.CategoryId.HasValue ? 
                (await _categoryRepository.GetByIdAsync(dto.CategoryId.Value, cancellationToken))?.Name ?? "Unknown" : 
                "All Categories";
            throw new BusinessRuleException("ValidationError", $"A budget for '{categoryName}' already exists for the specified date range.");
        }
    }

    public async Task ValidateBudgetBusinessRulesAsync(UpdateBudgetDto dto, CancellationToken cancellationToken = default)
    {
        // Check if budget exists
        var existingBudget = await _budgetRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (existingBudget == null)
        {
            throw new BusinessRuleException("ValidationError", $"Budget with ID {dto.Id} does not exist.");
        }

        // Validate category exists if specified
        if (dto.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId.Value, cancellationToken);
            if (category == null)
            {
                throw new BusinessRuleException("ValidationError", $"Category with ID {dto.CategoryId.Value} does not exist.");
            }

            if (!category.IsActive)
            {
                throw new BusinessRuleException("ValidationError", $"Cannot update budget for inactive category '{category.Name}'.");
            }
        }

        // Check for overlapping budgets for the same category (excluding current budget)
        var overlappingBudgets = await _budgetRepository.GetOverlappingBudgetsAsync(
            dto.CategoryId, dto.StartDate, dto.EndDate, cancellationToken);

        var conflictingBudgets = overlappingBudgets.Where(b => b.Id != dto.Id);
        if (conflictingBudgets.Any())
        {
            var categoryName = dto.CategoryId.HasValue ? 
                (await _categoryRepository.GetByIdAsync(dto.CategoryId.Value, cancellationToken))?.Name ?? "Unknown" : 
                "All Categories";
            throw new BusinessRuleException("ValidationError", $"A budget for '{categoryName}' already exists for the specified date range.");
        }
    }

    public async Task ValidateGoalBusinessRulesAsync(CreateGoalDto dto, CancellationToken cancellationToken = default)
    {
        // Validate linked account exists if specified
        if (dto.LinkedAccountId.HasValue)
        {
            var account = await _accountRepository.GetByIdAsync(dto.LinkedAccountId.Value, cancellationToken);
            if (account == null)
            {
                throw new BusinessRuleException("ValidationError", $"Linked account with ID {dto.LinkedAccountId.Value} does not exist.");
            }

            if (!account.IsActive)
            {
                throw new BusinessRuleException("ValidationError", $"Cannot link goal to inactive account '{account.Name}'.");
            }
        }

        // Check for duplicate goal names
        var existingGoal = await _goalRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (existingGoal != null)
        {
            throw new BusinessRuleException("ValidationError", $"A goal with the name '{dto.Name}' already exists.");
        }
    }

    public async Task ValidateGoalBusinessRulesAsync(UpdateGoalDto dto, CancellationToken cancellationToken = default)
    {
        // Check if goal exists
        var existingGoal = await _goalRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (existingGoal == null)
        {
            throw new BusinessRuleException("ValidationError", $"Goal with ID {dto.Id} does not exist.");
        }

        // Validate linked account exists if specified
        if (dto.LinkedAccountId.HasValue)
        {
            var account = await _accountRepository.GetByIdAsync(dto.LinkedAccountId.Value, cancellationToken);
            if (account == null)
            {
                throw new BusinessRuleException("ValidationError", $"Linked account with ID {dto.LinkedAccountId.Value} does not exist.");
            }

            if (!account.IsActive)
            {
                throw new BusinessRuleException("ValidationError", $"Cannot link goal to inactive account '{account.Name}'.");
            }
        }

        // Check for duplicate goal names (excluding current goal)
        var duplicateGoal = await _goalRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (duplicateGoal != null && duplicateGoal.Id != dto.Id)
        {
            throw new BusinessRuleException("ValidationError", $"A goal with the name '{dto.Name}' already exists.");
        }
    }

    private static bool IsValidCurrencyCode(string currency)
    {
        // Basic validation - in a real application, you might check against ISO 4217 codes
        var validCurrencies = new[] { "USD", "EUR", "GBP", "CAD", "AUD", "JPY", "CHF", "CNY", "INR", "BRL" };
        return validCurrencies.Contains(currency.ToUpper());
    }

    private async Task<bool> WouldCreateCircularReference(int categoryId, int parentCategoryId, CancellationToken cancellationToken)
    {
        var currentParent = await _categoryRepository.GetByIdAsync(parentCategoryId, cancellationToken);
        
        while (currentParent != null)
        {
            if (currentParent.Id == categoryId)
            {
                return true; // Circular reference detected
            }
            
            if (currentParent.ParentCategoryId.HasValue)
            {
                currentParent = await _categoryRepository.GetByIdAsync(currentParent.ParentCategoryId.Value, cancellationToken);
            }
            else
            {
                break;
            }
        }
        
        return false;
    }
}

