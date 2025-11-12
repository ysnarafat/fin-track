# Model Architecture Documentation

## Overview

FinTrack uses a layered model architecture that separates domain entities from UI-specific models, providing clean separation of concerns and optimized data structures for different layers of the application.

## Architecture Layers

### Domain Layer Models (`FinTrack.Core`)
Located in `src/frontend/src/FinTrack.Core/Entities/`, these models represent the core business entities:

- **BaseEntity**: Abstract base class with audit fields and sync properties
- **Transaction**: Core transaction entity with categorization
- **Account**: Financial account entity with balance tracking
- **Category**: Hierarchical category system
- **Budget**: Domain budget entity (planned)
- **Goal**: Domain goal entity (planned)

### Shared Models (`FinTrack.Shared.Models`)
Located in `src/frontend/src/FinTrack.Shared/Models/`, these models provide structured data transfer and validation support across application layers:

#### BusinessValidationResult
```csharp
public class BusinessValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public void AddError(string error);
    public void AddWarning(string warning);
    public bool HasErrors { get; }
    public bool HasWarnings { get; }
    public string GetErrorsAsString();
    public string GetWarningsAsString();
}
```

**Key Features:**
- Structured validation feedback with separate error and warning collections
- Helper methods for adding and retrieving validation messages
- Boolean flags for quick validation status checks
- String concatenation methods for UI display

#### FinancialSummary
```csharp
public class FinancialSummary
{
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public decimal TotalCashAndChecking { get; set; }
    public decimal TotalSavings { get; set; }
    public decimal TotalInvestments { get; set; }
    public decimal TotalCreditCardDebt { get; set; }
    public decimal TotalLoans { get; set; }
    public int ActiveAccountCount { get; set; }
    public int OverdrawnAccountCount { get; set; }
    public Dictionary<AccountType, decimal> BalancesByType { get; set; } = new();
    public Dictionary<AccountType, int> AccountCountByType { get; set; } = new();
}
```

**Key Features:**
- Comprehensive financial overview with asset/liability breakdown
- Account type-specific totals for detailed analysis
- Statistical information (account counts, overdrawn accounts)
- Dictionary collections for flexible account type analysis

### UI Models (`FinTrack.Maui.Models`)
Located in `src/frontend/src/FinTrack.Maui/Models/`, these models are optimized for UI display and data binding:

#### BudgetModel
```csharp
public class BudgetModel
{
    // Core properties
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal BudgetLimit { get; set; }
    public decimal CurrentSpending { get; set; }
    
    // Calculated properties for UI
    public decimal RemainingAmount { get; }
    public double UtilizationPercentage { get; }
    public BudgetStatus Status { get; }
    public string StatusColor { get; }
    public string StatusText { get; }
    public bool IsCurrentPeriod { get; }
}
```

**Key Features:**
- Real-time status calculation based on spending vs. budget
- Color-coded status indicators (Good, On Track, Warning, Exceeded)
- Automatic period validation
- Utilization percentage for progress bars

#### Goal Model
```csharp
public class Goal : INotifyPropertyChanged
{
    // Core properties
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime TargetDate { get; set; }
    
    // Calculated properties
    public double ProgressPercentage { get; }
    public decimal RemainingAmount { get; }
    public GoalStatus Status { get; }
    public decimal RequiredMonthlySavings { get; }
    
    // Milestone support
    public List<GoalMilestone> Milestones { get; set; }
    public GoalMilestone? NextMilestone { get; }
}
```

**Key Features:**
- Progress tracking with percentage calculations
- Timeline-based status determination
- Milestone achievement tracking with GoalMilestone entities
- Required savings calculations
- Property change notifications for real-time UI updates

#### GoalMilestone Entity
```csharp
public class GoalMilestone : BaseEntity
{
    public int GoalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public int SortOrder { get; set; }
    public bool IsAchieved { get; set; }
    public DateTime? AchievedDate { get; set; }
    public Goal Goal { get; set; } = null!;
}
```

**Key Features:**
- Milestone tracking within financial goals
- Achievement status with date tracking
- Sortable milestone ordering
- Parent-child relationship with Goal entity

#### DashboardData
```csharp
public class DashboardData
{
    public decimal TotalBalance { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal MonthlySavings { get; }
    public decimal SavingsRate { get; }
    public DashboardTrends Trends { get; set; }
}
```

**Key Features:**
- Aggregated financial overview
- Calculated savings metrics
- Trend analysis support
- Recent transaction integration

## Status Enumerations

### BudgetStatus
- **Good**: < 50% utilization
- **OnTrack**: 50-79% utilization  
- **Warning**: 80-99% utilization
- **Exceeded**: ≥ 100% utilization

### GoalStatus
- **OnTrack**: Progress matches or exceeds timeline
- **Behind**: Progress is 50-80% of expected timeline
- **AtRisk**: Progress is < 50% of expected timeline
- **Overdue**: Past target date without completion
- **Completed**: Goal achieved

## Design Patterns

### Calculated Properties
UI models use calculated properties extensively to provide real-time computed values without storing redundant data:

```csharp
public decimal RemainingAmount => BudgetLimit - CurrentSpending;
public double UtilizationPercentage => BudgetLimit > 0 ? (double)(CurrentSpending / BudgetLimit * 100) : 0;
```

### Status-Based Styling
Models include color and text properties that map directly to UI styling:

```csharp
public string StatusColor => Status switch
{
    BudgetStatus.Good => "#10B981",      // Green
    BudgetStatus.OnTrack => "#F59E0B",   // Yellow
    BudgetStatus.Warning => "#F97316",   // Orange
    BudgetStatus.Exceeded => "#EF4444",  // Red
    _ => "#6B7280"                       // Gray
};
```

### Property Change Notifications
Models that require real-time UI updates implement `INotifyPropertyChanged`:

```csharp
public decimal CurrentAmount
{
    get => _currentAmount;
    set
    {
        if (_currentAmount != value)
        {
            _currentAmount = value;
            OnPropertyChanged(nameof(CurrentAmount));
            OnPropertyChanged(nameof(ProgressPercentage));
            OnPropertyChanged(nameof(RemainingAmount));
        }
    }
}
```

## Data Transfer Objects (DTOs)

### Application Layer DTOs (`FinTrack.Shared/DTOs`)
DTOs provide a clean contract for data transfer between layers and external APIs:

#### Transaction DTOs
- **CreateTransactionDto**: For creating new transactions with validation
- **UpdateTransactionDto**: For updating existing transactions with ID validation
- **TransactionDto**: For data retrieval and display

#### Account DTOs
- **CreateAccountDto**: Account creation with type-specific validation
- **UpdateAccountDto**: Account updates with business rule validation
- **AccountDto**: Account data retrieval

#### Category DTOs
- **CreateCategoryDto**: Category creation with hierarchy validation
- **UpdateCategoryDto**: Category updates with self-parenting prevention
- **CategoryDto**: Category data retrieval

### DTO Validation Features
- **Type-specific Rules**: Credit card accounts require credit limits
- **Conditional Validation**: Transfer transactions require destination accounts
- **Format Validation**: Currency codes, hex colors, reference numbers
- **Business Logic**: Prevent circular category hierarchies, same-account transfers

## Model Mapping Strategy

The application uses mapping between domain entities, DTOs, shared models, and UI models:

1. **Domain → DTO**: Convert domain entities to DTOs for API contracts
2. **DTO → Domain**: Convert validated DTOs to domain entities for persistence
3. **Domain → UI**: Convert domain entities to UI models for display
4. **Domain → Shared**: Convert domain entities to shared models for service responses
5. **Validation**: DTOs include comprehensive FluentValidation rules
6. **Caching**: UI models can be cached for performance optimization

### Service Layer Integration

The shared models provide a clean contract between the service layer and consuming components:

```csharp
// Account Service using shared models
public async Task<BusinessValidationResult> ValidateAccountAsync(Account account)
{
    var result = new BusinessValidationResult();
    
    // Perform validation logic
    if (string.IsNullOrWhiteSpace(account.Name))
    {
        result.AddError("Account name is required");
    }
    
    if (account.Balance < -10000)
    {
        result.AddWarning("Account balance is unusually low");
    }
    
    return result;
}

public async Task<FinancialSummary> GetFinancialSummaryAsync()
{
    // Calculate comprehensive financial overview
    return new FinancialSummary
    {
        TotalAssets = await CalculateTotalAssetsAsync(),
        TotalLiabilities = await CalculateTotalLiabilitiesAsync(),
        NetWorth = totalAssets - totalLiabilities,
        // ... additional calculations
    };
}
```

## Validation Architecture

### FluentValidation Integration
FinTrack uses FluentValidation for comprehensive business rule validation across all model layers:

#### Application Layer Validators (`FinTrack.Shared/Validators`)
```csharp
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
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .WithMessage("Transaction date cannot be more than 1 day in the future");

        // Transfer specific validation
        RuleFor(x => x.TransferToAccountId)
            .NotEqual(x => x.AccountId).WithMessage("Cannot transfer to the same account")
            .When(x => x.Type == TransactionType.Transfer && x.TransferToAccountId.HasValue);
    }
}
```

#### Validation Features
- **Fluent API**: Expressive, readable validation rules using method chaining
- **Conditional Validation**: Rules that apply based on specific conditions
- **Custom Validators**: Domain-specific validation logic for financial rules
- **Async Validation**: Support for database-dependent validation rules
- **Localization**: Multi-language validation error messages

#### Validation Scenarios

**Implemented Validators:**
- **Transaction Validation**: Amount limits, date ranges, transfer validation, reference number constraints
- **Account Validation**: Currency format validation, credit limit rules, account type constraints
- **Category Validation**: Hex color validation, hierarchy rules (preventing self-parenting), budget limit constraints

**Planned Validators:**
- **Budget Validation**: Period overlaps, spending limits, category assignments
- **Goal Validation**: Target dates, milestone logic, progress tracking rules

#### Integration Points
- **Application Services**: Validation before business logic execution
- **Repository Layer**: Data integrity validation before persistence
- **UI Layer**: Real-time validation feedback with detailed error messages
- **Dependency Injection**: Automatic validator registration and resolution

## Repository Interface Enhancements

### IAccountRepository Extensions
Recent enhancements to the `IAccountRepository` interface include additional methods for improved data access patterns:

#### New Methods
```csharp
/// <summary>
/// Gets an account by name
/// </summary>
/// <param name="name">Account name</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>Account if found, null otherwise</returns>
Task<Account?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

/// <summary>
/// Gets a transaction by ID
/// </summary>
/// <param name="transactionId">Transaction ID</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>Transaction if found, null otherwise</returns>
Task<Transaction?> GetTransactionByIdAsync(int transactionId, CancellationToken cancellationToken = default);
```

#### Use Cases
- **GetByNameAsync**: Enables user-friendly account lookups by name instead of ID
- **GetTransactionByIdAsync**: Provides direct transaction access through the account repository for related operations
- **Enhanced Querying**: Supports more flexible data access patterns for UI scenarios

#### Implementation Status
- ✅ Interface methods defined in `IAccountRepository`
- ✅ Implementation completed in `AccountRepository` class
- ⏳ Unit tests pending for new methods

## Future Enhancements

- **AutoMapper Integration**: Automated mapping between domain and UI models
- **Advanced Validation Rules**: Complex cross-entity validation scenarios
- **Localization Support**: Culture-specific formatting and text
- **Theme Integration**: Dynamic color schemes based on user preferences
- **Validation Caching**: Performance optimization for expensive validation rules
- **Repository Method Implementation**: Complete implementation of new IAccountRepository methods