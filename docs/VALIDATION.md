# Validation Framework Documentation

FinTrack implements a comprehensive validation framework using FluentValidation to ensure data integrity and enforce business rules across the application.

## Architecture Overview

The validation framework is built around multiple service interfaces that provide different levels of validation functionality:

```
┌─────────────────────────────────────────────────────────────┐
│                    Validation Services                      │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │ IValidationSvc  │  │ IBusinessRule   │  │ IValidation  │ │
│  │                 │  │ ValidationSvc   │  │ ErrorSvc     │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    FluentValidation                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │ Account         │  │ Transaction     │  │ Category     │ │
│  │ Validators      │  │ Validators      │  │ Validators   │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                         DTOs                                │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │ CreateDTOs      │  │ UpdateDTOs      │  │ QueryDTOs    │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Service Interfaces

### IValidationService

The core validation service interface that provides basic validation operations:

```csharp
public interface IValidationService
{
    Task<ValidationResult> ValidateAsync<T>(T instance, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateAsync<T>(T instance, IValidator<T> validator, CancellationToken cancellationToken = default);
    Task<bool> IsValidAsync<T>(T instance, CancellationToken cancellationToken = default);
    Task ThrowIfInvalidAsync<T>(T instance, CancellationToken cancellationToken = default);
}
```

**Key Features:**
- Automatic validator resolution through dependency injection
- Async validation with cancellation token support
- Convenience methods for common validation scenarios
- Exception throwing for invalid instances

### IBusinessRuleValidationService

Specialized service for complex business rule validation scenarios:

```csharp
public interface IBusinessRuleValidationService
{
    Task<ValidationResult> ValidateBusinessRulesAsync<T>(T instance, CancellationToken cancellationToken = default);
    Task<bool> ValidateTransferRulesAsync(CreateTransactionDto transaction, CancellationToken cancellationToken = default);
    Task<bool> ValidateBudgetConstraintsAsync(CreateBudgetDto budget, CancellationToken cancellationToken = default);
    Task<bool> ValidateAccountBalanceRulesAsync(UpdateAccountDto account, CancellationToken cancellationToken = default);
}
```

**Key Features:**
- Domain-specific business rule validation
- Cross-entity validation scenarios
- Financial constraint validation
- Complex conditional logic

### IValidationErrorService

Service for handling and formatting validation errors:

```csharp
public interface IValidationErrorService
{
    string FormatValidationErrors(ValidationResult result);
    Dictionary<string, List<string>> GroupErrorsByProperty(ValidationResult result);
    ValidationResult CombineResults(params ValidationResult[] results);
    bool HasCriticalErrors(ValidationResult result);
}
```

**Key Features:**
- Error message formatting and localization
- Property-based error grouping
- Result combination for complex scenarios
- Critical error detection

### IComprehensiveValidationService

Combined validation service for complex scenarios requiring multiple validation types:

```csharp
public interface IComprehensiveValidationService
{
    Task<ValidationResult> ValidateCompletelyAsync<T>(T instance, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateWithBusinessRulesAsync<T>(T instance, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateForOperationAsync<T>(T instance, string operation, CancellationToken cancellationToken = default);
}
```

**Key Features:**
- Multi-layered validation approach
- Operation-specific validation rules
- Combined DTO and business rule validation
- Context-aware validation scenarios

## Validator Implementations

### Account Validators

#### CreateAccountDtoValidator
Validates new account creation with the following rules:

**Name Validation:**
- Required field
- Length between 1-100 characters

**Currency Validation:**
- Required field
- Exactly 3 characters
- Must match pattern `^[A-Z]{3}$` (e.g., USD, EUR, GBP)

**Type Validation:**
- Must be valid AccountType enum value

**Optional Field Validation:**
- Description: Maximum 500 characters
- Account Number: Maximum 50 characters
- Institution: Maximum 100 characters
- Credit Limit: Non-negative, required for credit cards
- Interest Rate: 0-100% range

**Credit Card Specific Rules:**
- Credit limit is required and must be greater than 0

#### UpdateAccountDtoValidator
Similar to create validator with additional ID validation:
- Account ID must be greater than 0
- All other rules identical to create validator

### Transaction Validators

#### CreateTransactionDtoValidator
Validates new transaction creation:

**Amount Validation:**
- Must be greater than 0
- Precision validation for currency amounts

**Description Validation:**
- Required field
- Maximum 500 characters

**Date Validation:**
- Cannot be in the future beyond tomorrow
- Must be valid date format

**Account and Category Validation:**
- Account ID must be greater than 0
- Category ID must be greater than 0
- Account and category must exist (business rule validation)

**Transfer Specific Rules:**
- Transfer to account ID required for transfer transactions
- Cannot transfer to the same account
- Transfer amount validation

#### UpdateTransactionDtoValidator
Similar to create validator with additional ID validation and update-specific rules.

### Category Validators

#### CreateCategoryDtoValidator
Validates new category creation:

**Name Validation:**
- Required field
- Length between 1-100 characters
- Unique within parent category scope

**Type Validation:**
- Must be valid TransactionType enum (Income/Expense)

**Hierarchy Validation:**
- Parent category ID validation
- Circular reference prevention
- Maximum hierarchy depth enforcement

**Visual Properties:**
- Color: Valid hex color format (#RRGGBB)
- Icon: Maximum 50 characters

**Budget Constraints:**
- Budget limit: Non-negative if specified

#### UpdateCategoryDtoValidator
Similar to create validator with additional update-specific rules and ID validation.

### Budget Validators

#### CreateBudgetDtoValidator
Validates new budget creation:

**Name and Amount:**
- Name required, 1-100 characters
- Amount must be greater than 0

**Period Validation:**
- Valid BudgetPeriod enum value
- Start date before end date
- Period duration validation

**Category Assignment:**
- Category ID must exist
- Category must be expense type
- No overlapping budgets for same category/period

**Alert Configuration:**
- Alert threshold between 0-1 (0-100%)
- Valid alert type enumeration

#### UpdateBudgetDtoValidator
Similar to create validator with ID validation and update-specific business rules.

### Goal Validators

#### CreateGoalDtoValidator
Validates new goal creation:

**Basic Properties:**
- Name required, 1-100 characters
- Target amount greater than 0
- Target date in the future

**Goal Type Validation:**
- Valid GoalType enum value
- Type-specific validation rules

**Progress Tracking:**
- Current amount non-negative
- Current amount not exceeding target (unless completed)

**Account Linking:**
- Linked account ID validation
- Account type compatibility with goal type

#### UpdateGoalDtoValidator
Similar to create validator with additional progress update validation and completion rules.

## Configuration and Setup

### Basic Registration

Register all validation services with default configuration:

```csharp
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
    // ... additional validator registrations

    return services;
}
```

### Advanced Configuration

Configure validation services with custom options:

```csharp
services.AddValidationServices(config => {
    config.ServiceLifetime = ServiceLifetime.Scoped;
    config.ValidatorOptions = new ValidationOptions {
        DefaultClassLevelCascadeMode = CascadeMode.Continue,
        DefaultRuleLevelCascadeMode = CascadeMode.Stop
    };
});
```

**Configuration Options:**

- **ServiceLifetime**: Controls the lifetime of validator instances (Scoped, Transient, Singleton)
- **DefaultClassLevelCascadeMode**: How validation continues when class-level rules fail
- **DefaultRuleLevelCascadeMode**: How validation continues when individual rules fail

### Cascade Modes

**CascadeMode.Continue**: Continue validation even after failures
- Collects all validation errors
- Provides comprehensive feedback
- Better for user experience

**CascadeMode.Stop**: Stop validation on first failure
- Faster validation execution
- Reduces processing overhead
- Good for critical validation scenarios

## Usage Examples

### Basic Validation

```csharp
public class AccountService
{
    private readonly IValidationService _validationService;

    public async Task<Account> CreateAccountAsync(CreateAccountDto dto)
    {
        // Validate the DTO
        var validationResult = await _validationService.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Proceed with account creation
        // ...
    }
}
```

### Business Rule Validation

```csharp
public class TransactionService
{
    private readonly IBusinessRuleValidationService _businessRuleValidation;

    public async Task<Transaction> CreateTransferAsync(CreateTransactionDto dto)
    {
        // Validate transfer-specific business rules
        var isValidTransfer = await _businessRuleValidation
            .ValidateTransferRulesAsync(dto);
        
        if (!isValidTransfer)
        {
            throw new BusinessRuleException("Invalid transfer operation");
        }

        // Proceed with transfer creation
        // ...
    }
}
```

### Comprehensive Validation

```csharp
public class BudgetService
{
    private readonly IComprehensiveValidationService _comprehensiveValidation;

    public async Task<Budget> CreateBudgetAsync(CreateBudgetDto dto)
    {
        // Perform complete validation including business rules
        var result = await _comprehensiveValidation
            .ValidateCompletelyAsync(dto);
        
        if (!result.IsValid)
        {
            var formattedErrors = _validationErrorService
                .FormatValidationErrors(result);
            throw new ValidationException(formattedErrors);
        }

        // Proceed with budget creation
        // ...
    }
}
```

### Error Handling and Formatting

```csharp
public class ValidationController
{
    private readonly IValidationErrorService _errorService;

    public async Task<IActionResult> ValidateData<T>(T data)
    {
        var result = await _validationService.ValidateAsync(data);
        
        if (!result.IsValid)
        {
            // Group errors by property for UI display
            var groupedErrors = _errorService.GroupErrorsByProperty(result);
            
            // Format errors for user display
            var formattedMessage = _errorService.FormatValidationErrors(result);
            
            // Check for critical errors
            var hasCriticalErrors = _errorService.HasCriticalErrors(result);
            
            return BadRequest(new {
                Errors = groupedErrors,
                Message = formattedMessage,
                IsCritical = hasCriticalErrors
            });
        }

        return Ok();
    }
}
```

## Best Practices

### Validator Design

1. **Single Responsibility**: Each validator should focus on one DTO type
2. **Reusable Rules**: Extract common validation logic into extension methods
3. **Clear Messages**: Provide specific, actionable error messages
4. **Conditional Logic**: Use `When()` clauses for context-specific rules

### Performance Considerations

1. **Async Validation**: Always use async methods for database-dependent validation
2. **Cancellation Tokens**: Support cancellation for long-running validation
3. **Caching**: Cache expensive validation results when appropriate
4. **Lazy Loading**: Avoid unnecessary database queries in validators

### Error Handling

1. **Structured Errors**: Use property-specific error grouping
2. **User-Friendly Messages**: Avoid technical jargon in user-facing errors
3. **Localization**: Support multiple languages for error messages
4. **Logging**: Log validation failures for debugging and monitoring

### Testing

1. **Unit Tests**: Test each validator independently
2. **Integration Tests**: Test validation in service layer context
3. **Edge Cases**: Test boundary conditions and edge cases
4. **Performance Tests**: Validate performance under load

## Integration with Application Layers

### Service Layer Integration

```csharp
public class BaseService<TEntity, TCreateDto, TUpdateDto>
{
    protected readonly IValidationService _validationService;

    protected async Task ValidateAsync<T>(T dto)
    {
        await _validationService.ThrowIfInvalidAsync(dto);
    }
}
```

### Repository Layer Integration

```csharp
public class BaseRepository<T> where T : BaseEntity
{
    protected async Task ValidateEntityAsync(T entity)
    {
        if (!entity.IsValid())
        {
            throw new DomainException("Entity validation failed");
        }
    }
}
```

### UI Layer Integration

```csharp
public class BaseViewModel : INotifyPropertyChanged
{
    protected readonly IValidationService _validationService;

    protected async Task<bool> ValidateAsync<T>(T model)
    {
        var result = await _validationService.ValidateAsync(model);
        
        // Update UI with validation results
        UpdateValidationErrors(result);
        
        return result.IsValid;
    }
}
```

## Future Enhancements

### Planned Features

1. **Real-time Validation**: Live validation as user types
2. **Conditional Validation**: Dynamic rules based on application state
3. **Custom Rule Engine**: Configurable business rules
4. **Validation Caching**: Performance optimization for repeated validations
5. **Audit Trail**: Track validation history for compliance

### Extension Points

1. **Custom Validators**: Framework for domain-specific validators
2. **Validation Middleware**: Pipeline-based validation processing
3. **Rule Composition**: Combine multiple validation rules dynamically
4. **External Validation**: Integration with external validation services

---

This validation framework provides a robust foundation for ensuring data integrity and enforcing business rules throughout the FinTrack application while maintaining flexibility for future enhancements and customizations.