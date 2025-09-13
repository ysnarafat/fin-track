# FinTrack API Documentation

## Infrastructure Layer Enhancements

### BaseRepository Hard Delete Enhancement

The `BaseRepository<T>` class has been enhanced with an improved hard delete implementation that uses Entity Framework's `ExecuteDeleteAsync` method for better performance and reliability.

#### Enhanced HardDeleteAsync Method

The `HardDeleteAsync()` method now uses direct database operations for permanent entity removal:

```csharp
public virtual async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
{
    try
    {
        _logger.LogDebug("Hard deleting {EntityType} entity with ID {Id}", typeof(T).Name, id);
        
        // Use ExecuteDeleteAsync for hard delete to bypass soft delete logic
        var deletedCount = await _dbSet
            .IgnoreQueryFilters()
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        
        _logger.LogDebug("Hard deleted {EntityType} entity with ID {Id}", typeof(T).Name, id);
        return deletedCount > 0;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error hard deleting {EntityType} entity with ID {Id}", typeof(T).Name, id);
        throw;
    }
}
```

#### Key Improvements

1. **Direct Database Operation**: Uses `ExecuteDeleteAsync` to perform deletion directly at the database level without loading entities into memory
2. **Performance Enhancement**: Eliminates Entity Framework change tracking overhead for hard delete operations
3. **Soft Delete Bypass**: Properly uses `IgnoreQueryFilters()` to ensure hard delete works on entities that are already soft-deleted
4. **Atomic Operation**: Single database command execution reduces concurrency issues and ensures data consistency
5. **Accurate Return Values**: Returns `deletedCount > 0` to accurately reflect whether any entities were actually deleted

#### Benefits

- **Better Performance**: No entity loading or change tracking overhead
- **Memory Efficiency**: Avoids loading entities into memory just to delete them
- **Atomic Operations**: Single database command execution
- **Proper Soft Delete Handling**: Correctly bypasses soft delete mechanisms when permanent removal is required
- **Accurate Results**: Returns actual count of affected entities

#### Usage Scenarios

Hard delete should be used sparingly and only when:
- Permanent data removal is required (e.g., GDPR compliance)
- Cleaning up test data in development environments
- Administrative operations that require complete data removal
- Maintenance operations on obsolete or corrupted data

**Note**: Hard delete permanently removes data from the database and cannot be undone. Use soft delete (`DeleteAsync`) for normal application operations to maintain data integrity and audit trails.

### Database Context Synchronization Improvements

The `FinTrackDbContext` has been enhanced with intelligent audit field management to prevent conflicts between repository operations and automatic database updates.

#### Enhanced UpdateAuditFields Method

The `UpdateAuditFields()` method has been streamlined for better reliability and consistency:

```csharp
private void UpdateAuditFields()
{
    var entries = ChangeTracker.Entries<BaseEntity>();
    var now = DateTime.UtcNow;

    foreach (var entry in entries)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                // Set CreatedAt and UpdatedAt if they are default values
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = now;
                }
                if (entry.Entity.UpdatedAt == default)
                {
                    entry.Entity.UpdatedAt = now;
                }
                if (string.IsNullOrEmpty(entry.Entity.SyncId))
                {
                    entry.Entity.SyncId = Guid.NewGuid().ToString();
                }
                // Set initial version
                if (entry.Entity.Version == 0)
                {
                    entry.Entity.Version = 1;
                }
                // Only set sync status if it's the default value
                if (entry.Entity.SyncStatus == default(SyncStatus))
                {
                    entry.Entity.SyncStatus = SyncStatus.PendingCreate;
                }
                break;
            case EntityState.Modified:
                // Always update UpdatedAt for modifications
                entry.Entity.UpdatedAt = now;
                entry.Entity.Version++;
                
                // Check if SyncStatus was explicitly modified
                var syncStatusProperty = entry.Property(nameof(BaseEntity.SyncStatus));
                if (!syncStatusProperty.IsModified)
                {
                    // Only change sync status if it's currently Synced
                    if (entry.Entity.SyncStatus == SyncStatus.Synced)
                    {
                        entry.Entity.SyncStatus = SyncStatus.PendingUpdate;
                    }
                }
                break;
            case EntityState.Deleted:
                // Check if this is a hard delete (marked with HardDelete sync status)
                if (entry.Entity.SyncStatus == SyncStatus.HardDelete)
                {
                    // Allow hard delete to proceed
                    break;
                }
                
                // For soft delete, change to modified and set IsDeleted
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.UpdatedAt = now;
                entry.Entity.Version++;
                entry.Entity.SyncStatus = SyncStatus.PendingDelete;
                break;
        }
    }
}
```

#### Key Improvements

1. **Simplified Logic**: Uses direct value checks instead of complex property modification tracking for better reliability
2. **Always Update Timestamps**: UpdatedAt is always set for modifications, ensuring accurate audit trails
3. **Smart Sync Status Management**: Only changes sync status from Synced to PendingUpdate, preserving other states
4. **Enhanced Hard Delete Support**: Proper handling of hard delete operations with SyncStatus.HardDelete bypass logic
5. **Consistent Version Management**: Version is always incremented for modifications with proper initial version setting

#### Repository Integration

Repositories can now explicitly control audit properties by marking them as modified:

```csharp
// In BaseRepository.MarkAsSyncedAsync
foreach (var entity in entities)
{
    entity.MarkAsSynced();
    // Mark sync status as modified to prevent override in UpdateAuditFields
    _context.Entry(entity).Property(nameof(BaseEntity.SyncStatus)).IsModified = true;
}
```

This ensures that repository-level sync operations are not overridden by the DbContext automatic updates.

## Service Layer Architecture

The FinTrack application follows a service-oriented architecture with a clear separation between layers:

### Service Layer Separation
- **Shared Layer Services** (`FinTrack.Shared.Services`): Work with domain entities and provide business logic
- **UI Layer Services** (`FinTrack.Maui.Services`): Work with UI models and provide presentation-specific operations

This separation allows for:
- Clean domain logic without UI concerns
- UI-optimized data models and operations
- Testable business logic independent of presentation
- Future API layer integration without UI dependencies

### IBudgetService Implementations

There are two IBudgetService interfaces serving different purposes:

1. **Domain Service** (`FinTrack.Shared.Services.IBudgetService`): Works with `Budget` entities
2. **UI Service** (`FinTrack.Maui.Services.IBudgetService`): Works with `BudgetModel` UI models

The UI service typically wraps the domain service and handles model transformation.

## Account Service API

### IAccountService Interface

The `IAccountService` (`FinTrack.Shared.Services.IAccountService`) provides comprehensive account management capabilities with business rule validation, financial calculations, and relationship management. This service integrates with the `FinTrack.Shared.Models` namespace for structured validation responses and financial summaries.

#### Core CRUD Operations

##### CreateAccountAsync
```csharp
Task<Account> CreateAccountAsync(Account account, CancellationToken cancellationToken = default)
```
Creates a new account with comprehensive validation and automatic initial balance setting.

**Features:**
- Advanced validation using `ValidateAccountAsync`
- Automatic initial balance assignment from current balance
- Business rule enforcement (duplicate names, account type validation)
- Comprehensive logging

**Validation Rules:**
- Account data integrity validation
- Duplicate name detection (case-insensitive)
- Credit card specific validation (credit limit requirements)
- Loan account validation (balance expectations)
- Investment account validation (interest rate constraints)
- Balance reasonableness checks (warnings for extreme values)

**Throws:**
- `BusinessRuleException` when validation fails

##### UpdateAccountAsync
```csharp
Task<Account> UpdateAccountAsync(Account account, CancellationToken cancellationToken = default)
```
Updates an existing account with the same validation rules as creation.

##### DeleteAccountAsync
```csharp
Task<bool> DeleteAccountAsync(int accountId, CancellationToken cancellationToken = default)
```
Soft deletes an account after validating business rules.

**Business Rules:**
- Cannot delete accounts with associated transactions
- Cannot delete accounts with linked financial goals
- Returns `false` if account not found

**Throws:**
- `BusinessRuleException` when deletion is prevented by business rules

#### Query Operations

##### GetAccountAsync
```csharp
Task<Account?> GetAccountAsync(int accountId, CancellationToken cancellationToken = default)
```
Retrieves a single account by ID. Returns `null` if not found.

##### GetActiveAccountsAsync
```csharp
Task<IEnumerable<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
```
Returns all active (non-deleted, IsActive = true) accounts.

##### GetAccountsByTypeAsync
```csharp
Task<IEnumerable<Account>> GetAccountsByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default)
```
Returns accounts filtered by account type (Checking, Savings, Credit Card, etc.).

##### GetAccountsWithRecentTransactionsAsync
```csharp
Task<IEnumerable<Account>> GetAccountsWithRecentTransactionsAsync(int transactionCount = 5, CancellationToken cancellationToken = default)
```
Returns accounts with their most recent transactions loaded for display.

##### SearchAccountsAsync
```csharp
Task<IEnumerable<Account>> SearchAccountsAsync(string searchTerm, CancellationToken cancellationToken = default)
```
Searches accounts by name or institution using the provided search term.

#### Financial Operations

##### RecalculateAccountBalanceAsync
```csharp
Task<decimal> RecalculateAccountBalanceAsync(int accountId, CancellationToken cancellationToken = default)
```
Recalculates account balance based on transactions and updates the account.

**Calculation Logic:**
- Starts with initial balance
- Adds income transactions
- Subtracts expense transactions
- Handles transfer transactions (in and out)
- Updates account balance and persists changes

##### GetBalanceHistoryAsync
```csharp
Task<Dictionary<DateTime, decimal>> GetBalanceHistoryAsync(int accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
```
Returns daily balance history for an account over the specified date range.

##### GetOverdrawnAccountsAsync
```csharp
Task<IEnumerable<Account>> GetOverdrawnAccountsAsync(CancellationToken cancellationToken = default)
```
Returns accounts that are overdrawn or over their credit limit.

##### GetFinancialSummaryAsync
```csharp
Task<FinancialSummary> GetFinancialSummaryAsync(CancellationToken cancellationToken = default)
```
Generates comprehensive financial summary across all accounts.

**Summary Includes:**
- Total assets and liabilities
- Net worth calculation
- Account counts by type
- Balance totals by account type
- Overdrawn account count
- Specific totals (cash, savings, investments, debt)

##### GetAccountSummaryAsync
```csharp
Task<AccountSummary?> GetAccountSummaryAsync(int accountId, CancellationToken cancellationToken = default)
```
Returns detailed summary for a specific account including transaction statistics.

#### Management Operations

##### SetAccountActiveStatusAsync
```csharp
Task<bool> SetAccountActiveStatusAsync(int accountId, bool isActive, CancellationToken cancellationToken = default)
```
Activates or deactivates an account. Returns `true` if successful, `false` if account not found.

#### Repository Layer Extensions

The `AccountRepository` has been enhanced with additional query methods for improved data access:

##### GetByNameAsync
```csharp
Task<Account?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
```
Retrieves an account by its exact name. This method provides user-friendly account lookups and supports account selection scenarios in the UI.

**Features:**
- Case-sensitive name matching
- Excludes soft-deleted accounts
- Returns `null` if no matching account is found
- Comprehensive logging for debugging

**Use Cases:**
- Account selection by name in transaction forms
- Account validation during data import
- User-friendly account lookups in search scenarios

##### GetTransactionByIdAsync
```csharp
Task<Transaction?> GetTransactionByIdAsync(int transactionId, CancellationToken cancellationToken = default)
```
Retrieves a transaction by its ID directly through the account repository. This method provides convenient transaction access when working within account-centric operations.

**Features:**
- Direct transaction retrieval without requiring transaction repository
- Excludes soft-deleted transactions
- Returns `null` if transaction not found
- Supports account-related transaction operations

**Use Cases:**
- Transaction validation during account operations
- Account-centric transaction management
- Simplified transaction access in account services

#### Validation Operations

##### ValidateAccountAsync
```csharp
Task<BusinessValidationResult> ValidateAccountAsync(Account account, CancellationToken cancellationToken = default)
```
Performs comprehensive validation of account data and business rules.

**Validation Categories:**
- **Data Integrity**: Basic entity validation using `account.IsValid()`
- **Business Rules**: Domain-specific validation rules
- **Duplicate Detection**: Name uniqueness validation (case-insensitive)
- **Account Type Validation**: Type-specific business rules
- **Warning Generation**: Unusual values and potential issues

**Account Type Specific Rules:**
- **Credit Card**: Credit limit validation, balance expectations
- **Loan**: Balance expectations (typically negative)
- **Investment**: Interest rate validation (non-negative)

## Budget Service API

### Domain Layer IBudgetService Interface

The domain layer `IBudgetService` (`FinTrack.Shared.Services.IBudgetService`) provides comprehensive budget management capabilities with advanced validation, alert generation, and performance analytics. This service works with domain entities and implements core business logic.

#### Core CRUD Operations

##### CreateBudgetAsync
```csharp
Task<Budget> CreateBudgetAsync(Budget budget, CancellationToken cancellationToken = default)
```
Creates a new budget with comprehensive validation and automatic spent amount calculation.

**Features:**
- Advanced validation using `ValidateBudgetAsync`
- Automatic spent amount calculation based on existing transactions
- Business rule enforcement (duplicate names, category validation, period validation)
- Comprehensive logging

**Validation Rules:**
- Budget data integrity validation
- Duplicate name detection within the same period
- Category existence verification
- Overlapping budget detection for the same category
- Period validation (minimum 1 day, warning for periods > 366 days)
- Amount validation (warning for amounts > $100,000)
- Alert threshold validation (warning for thresholds < 50%)

**Throws:**
- `BusinessRuleException` when validation fails

##### UpdateBudgetAsync
```csharp
Task<Budget> UpdateBudgetAsync(Budget budget, CancellationToken cancellationToken = default)
```
Updates an existing budget with the same validation rules as creation.

##### DeleteBudgetAsync
```csharp
Task<bool> DeleteBudgetAsync(int budgetId, CancellationToken cancellationToken = default)
```
Soft deletes a budget. Returns `true` if successful, `false` if budget not found.

##### GetBudgetAsync
```csharp
Task<Budget?> GetBudgetAsync(int budgetId, CancellationToken cancellationToken = default)
```
Retrieves a single budget by ID. Returns `null` if not found.

#### Query Operations

##### GetActiveBudgetsAsync
```csharp
Task<IEnumerable<Budget>> GetActiveBudgetsAsync(CancellationToken cancellationToken = default)
```
Returns all active (non-deleted, IsActive = true) budgets.

##### GetCurrentBudgetsAsync
```csharp
Task<IEnumerable<Budget>> GetCurrentBudgetsAsync(CancellationToken cancellationToken = default)
```
Returns budgets that are active and cover the current date.

##### GetBudgetsByPeriodAsync
```csharp
Task<IEnumerable<Budget>> GetBudgetsByPeriodAsync(BudgetPeriod period, CancellationToken cancellationToken = default)
```
Returns budgets filtered by period type (Weekly, Monthly, Quarterly, Annual, Custom).

##### GetBudgetsByCategoryAsync
```csharp
Task<IEnumerable<Budget>> GetBudgetsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
```
Returns all budgets associated with a specific category.

##### SearchBudgetsAsync
```csharp
Task<IEnumerable<Budget>> SearchBudgetsAsync(string searchTerm, CancellationToken cancellationToken = default)
```
Searches budgets by name using the provided search term.

#### Alert and Monitoring Operations

##### GetBudgetAlertsAsync
```csharp
Task<IEnumerable<BudgetAlert>> GetBudgetAlertsAsync(CancellationToken cancellationToken = default)
```
Generates intelligent budget alerts based on current budget status.

**Alert Types:**
- **BudgetExceeded**: Budget has been exceeded
- **ThresholdReached**: Alert threshold percentage reached
- **ProjectedOverspend**: Current spending rate projects budget will be exceeded
- **PeriodEnding**: Budget period ending within 7 days
- **NoActivity**: No spending activity detected (budget halfway through period)

**Alert Generation Logic:**
- Only processes budgets with `AlertsEnabled = true`
- Calculates real-time utilization percentages
- Projects future spending based on current rate
- Provides detailed messages with amounts and percentages

##### GetExceededBudgetsAsync
```csharp
Task<IEnumerable<Budget>> GetExceededBudgetsAsync(CancellationToken cancellationToken = default)
```
Returns budgets where spent amount exceeds the budget limit.

##### GetBudgetsAtAlertThresholdAsync
```csharp
Task<IEnumerable<Budget>> GetBudgetsAtAlertThresholdAsync(CancellationToken cancellationToken = default)
```
Returns budgets that have reached their configured alert threshold.

#### Performance and Analytics

##### GetBudgetPerformanceAsync
```csharp
Task<IEnumerable<BudgetPerformance>> GetBudgetPerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
```
Returns detailed performance analytics for budgets within the specified date range.

##### GetUtilizationStatsAsync
```csharp
Task<BudgetUtilizationStats> GetUtilizationStatsAsync(CancellationToken cancellationToken = default)
```
Returns comprehensive utilization statistics across all budgets.

#### Calculation Operations

##### RecalculateAllSpentAmountsAsync
```csharp
Task<int> RecalculateAllSpentAmountsAsync(CancellationToken cancellationToken = default)
```
Recalculates spent amounts for all active budgets based on current transactions. Returns the number of budgets updated.

**Use Cases:**
- Data synchronization after bulk transaction imports
- Periodic maintenance operations
- Recovery from data inconsistencies

##### RecalculateSpentAmountAsync
```csharp
Task<bool> RecalculateSpentAmountAsync(int budgetId, CancellationToken cancellationToken = default)
```
Recalculates spent amount for a specific budget. Returns `true` if the amount was updated.

**Calculation Logic:**
- For category-specific budgets: Sums expense transactions in the category within the budget period
- For general budgets (no category): Sums all expense transactions within the budget period
- Excludes soft-deleted transactions
- Updates budget entity and persists changes

#### Management Operations

##### SetBudgetActiveStatusAsync
```csharp
Task<bool> SetBudgetActiveStatusAsync(int budgetId, bool isActive, CancellationToken cancellationToken = default)
```
Activates or deactivates a budget. Returns `true` if successful, `false` if budget not found.

##### CreateNextPeriodBudgetsAsync
```csharp
Task<IEnumerable<Budget>> CreateNextPeriodBudgetsAsync(CancellationToken cancellationToken = default)
```
Automatically creates budgets for the next period based on current active budgets.

**Features:**
- Copies budget configuration (amount, category, alert settings)
- Calculates appropriate date ranges for the next period
- Only processes active budgets
- Returns the newly created budgets

#### Validation Operations

##### ValidateBudgetAsync
```csharp
Task<ValidationResult> ValidateBudgetAsync(Budget budget, CancellationToken cancellationToken = default)
```
Performs comprehensive validation of budget data and business rules.

**Validation Categories:**
- **Data Integrity**: Basic entity validation
- **Business Rules**: Domain-specific validation
- **Relationship Validation**: Category existence and type validation
- **Conflict Detection**: Duplicate names and overlapping periods
- **Warning Generation**: Unusual values and potential issues

## Supporting Classes

### BusinessValidationResult
```csharp
public class BusinessValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public void AddError(string error);
    public void AddWarning(string warning);
}
```

Provides structured validation feedback for account operations with separate error and warning collections.

### FinancialSummary
```csharp
public class FinancialSummary
{
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public int ActiveAccountCount { get; set; }
    public int OverdrawnAccountCount { get; set; }
    
    // Account type specific totals
    public decimal TotalCashAndChecking { get; set; }
    public decimal TotalSavings { get; set; }
    public decimal TotalInvestments { get; set; }
    public decimal TotalCreditCardDebt { get; set; }
    public decimal TotalLoans { get; set; }
    
    // Collections for detailed breakdown
    public Dictionary<AccountType, decimal> BalancesByType { get; set; } = new();
    public Dictionary<AccountType, int> AccountCountByType { get; set; } = new();
}
```

Comprehensive financial overview with asset/liability breakdown and account type analysis.

### AccountSummary
```csharp
public class AccountSummary
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public int TransactionCount { get; set; }
    public DateTime? FirstTransactionDate { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public decimal MonthToDateIncome { get; set; }
    public decimal MonthToDateExpenses { get; set; }
    public decimal YearToDateIncome { get; set; }
    public decimal YearToDateExpenses { get; set; }
}
```

Detailed account statistics including transaction history and period-based income/expense totals.

### ValidationResult
```csharp
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public void AddError(string error);
    public void AddWarning(string warning);
}
```

Provides structured validation feedback with separate error and warning collections.

### BudgetAlert
```csharp
public class BudgetAlert
{
    public Budget Budget { get; set; } = null!;
    public BudgetAlertType AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

Represents a budget alert with detailed information about the alert condition.

### BudgetAlertType Enumeration
```csharp
public enum BudgetAlertType
{
    BudgetExceeded,      // Spending has exceeded the budget limit
    ThresholdReached,    // Alert threshold percentage reached
    ProjectedOverspend,  // Current rate projects budget will be exceeded
    PeriodEnding,        // Budget period ending soon (within 7 days)
    NoActivity          // No spending activity detected
}
```

## Error Handling

### Exception Types
- **BusinessRuleException**: Thrown when business rules are violated during budget operations
- **EntityNotFoundException**: Thrown when referenced entities (categories) are not found
- **ConcurrencyException**: Thrown when optimistic concurrency conflicts occur

### Logging
All service operations include comprehensive logging:
- **Information**: Successful operations with key details
- **Warning**: Business rule violations and not-found scenarios
- **Error**: Exception scenarios with full exception details
- **Debug**: Detailed operation flow for troubleshooting

## Usage Examples

### Creating an Account with Validation
```csharp
var account = new Account
{
    Name = "Primary Checking",
    Type = AccountType.Checking,
    Currency = "USD",
    Balance = 1500.00m,
    Institution = "Sample Bank",
    IsActive = true
};

try
{
    var createdAccount = await accountService.CreateAccountAsync(account);
    Console.WriteLine($"Account created with ID: {createdAccount.Id}");
}
catch (BusinessRuleException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

### Generating Financial Summary
```csharp
var summary = await accountService.GetFinancialSummaryAsync();

Console.WriteLine($"Net Worth: {summary.NetWorth:C}");
Console.WriteLine($"Total Assets: {summary.TotalAssets:C}");
Console.WriteLine($"Total Liabilities: {summary.TotalLiabilities:C}");
Console.WriteLine($"Active Accounts: {summary.ActiveAccountCount}");

// Account type breakdown
foreach (var (accountType, balance) in summary.BalancesByType)
{
    Console.WriteLine($"{accountType}: {balance:C}");
}
```

### Account Deletion with Business Rule Validation
```csharp
try
{
    var deleted = await accountService.DeleteAccountAsync(accountId);
    if (deleted)
    {
        Console.WriteLine("Account deleted successfully");
    }
    else
    {
        Console.WriteLine("Account not found");
    }
}
catch (BusinessRuleException ex) when (ex.RuleName == "AccountHasTransactions")
{
    Console.WriteLine("Cannot delete account: it has associated transactions");
}
catch (BusinessRuleException ex) when (ex.RuleName == "AccountHasLinkedGoals")
{
    Console.WriteLine("Cannot delete account: it has linked financial goals");
}
```

### Recalculating Account Balance
```csharp
// Recalculate balance after transaction changes
var newBalance = await accountService.RecalculateAccountBalanceAsync(accountId);
Console.WriteLine($"Account balance recalculated: {newBalance:C}");

// Get balance history for reporting
var startDate = DateTime.Today.AddMonths(-3);
var endDate = DateTime.Today;
var history = await accountService.GetBalanceHistoryAsync(accountId, startDate, endDate);

foreach (var (date, balance) in history)
{
    Console.WriteLine($"{date:yyyy-MM-dd}: {balance:C}");
}
```

### Creating a Budget with Validation
```csharp
var budget = new Budget
{
    Name = "Monthly Groceries",
    Amount = 800.00m,
    Period = BudgetPeriod.Monthly,
    StartDate = new DateTime(2024, 1, 1),
    EndDate = new DateTime(2024, 1, 31),
    CategoryId = 1, // Food & Dining category
    AlertThreshold = 0.8m,
    IsActive = true
};

try
{
    var createdBudget = await budgetService.CreateBudgetAsync(budget);
    Console.WriteLine($"Budget created with ID: {createdBudget.Id}");
}
catch (BusinessRuleException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

### Generating Budget Alerts
```csharp
var alerts = await budgetService.GetBudgetAlertsAsync();

foreach (var alert in alerts)
{
    switch (alert.AlertType)
    {
        case BudgetAlertType.BudgetExceeded:
            Console.WriteLine($"⚠️ {alert.Message}");
            break;
        case BudgetAlertType.ThresholdReached:
            Console.WriteLine($"📊 {alert.Message}");
            break;
        case BudgetAlertType.PeriodEnding:
            Console.WriteLine($"⏰ {alert.Message}");
            break;
    }
}
```

### Recalculating Budget Amounts
```csharp
// Recalculate all budgets after transaction import
var updatedCount = await budgetService.RecalculateAllSpentAmountsAsync();
Console.WriteLine($"Updated {updatedCount} budgets");

// Recalculate specific budget
var wasUpdated = await budgetService.RecalculateSpentAmountAsync(budgetId);
if (wasUpdated)
{
    Console.WriteLine("Budget amount recalculated successfully");
}
```

## Integration Points

### Account Service Dependencies

#### Repository Dependencies
- **IAccountRepository**: Core account data operations and balance calculations
- **ITransactionRepository**: Transaction data for balance calculations and validation
- **IGoalRepository**: Goal data for account deletion validation

#### Service Dependencies
- **ILogger<AccountService>**: Comprehensive logging throughout all operations
- **Dependency Injection**: Registered as scoped service in the DI container

#### UI Integration
- **AccountsViewModel**: Consumes account service for UI operations
- **Dashboard**: Uses financial summary and account status data
- **Account Management Pages**: Full CRUD operations with validation feedback
- **Transaction Forms**: Account selection and validation

### Budget Service Dependencies

#### Repository Dependencies
- **IBudgetRepository**: Core budget data operations
- **ICategoryRepository**: Category validation and lookup
- **ITransactionRepository**: Transaction data for spent amount calculations

#### Service Dependencies
- **ILogger<BudgetService>**: Comprehensive logging throughout all operations
- **Dependency Injection**: Registered as scoped service in the DI container

#### UI Integration
- **BudgetsViewModel**: Consumes budget service for UI operations
- **Dashboard**: Uses budget alerts and performance data
- **Budget Management Pages**: Full CRUD operations with validation feedback

## Performance Considerations

### Optimization Strategies
- **Lazy Loading**: Repository queries use appropriate includes for navigation properties
- **Batch Operations**: RecalculateAllSpentAmountsAsync processes multiple budgets efficiently
- **Caching**: Consider implementing caching for frequently accessed budget data
- **Async Operations**: All operations are fully asynchronous with cancellation token support

### Scalability Notes
- **Database Indexes**: Ensure proper indexing on budget date ranges and category relationships
- **Query Optimization**: Spent amount calculations use efficient LINQ queries
- **Memory Management**: Large result sets should consider pagination for UI scenarios

## UI Layer Budget Service

### IBudgetService Interface (UI Layer)

The UI layer `IBudgetService` (`FinTrack.Maui.Services.IBudgetService`) provides presentation-optimized budget operations working with `BudgetModel` objects.

#### Key Methods

##### GetCurrentMonthBudgetsAsync
```csharp
Task<IEnumerable<BudgetModel>> GetCurrentMonthBudgetsAsync()
```
Returns budgets for the current month optimized for UI display with calculated properties.

##### GetBudgetSummaryAsync
```csharp
Task<BudgetSummary> GetBudgetSummaryAsync()
```
Returns aggregated budget data for dashboard display.

##### GetBudgetAlertsAsync
```csharp
Task<IEnumerable<BudgetAlert>> GetBudgetAlertsAsync()
```
Returns UI-optimized budget alerts with simplified alert types (Warning, Exceeded).

##### GetAvailableCategoriesAsync
```csharp
Task<IEnumerable<CategoryOption>> GetAvailableCategoriesAsync()
```
Returns categories available for budget creation with existing budget indicators.

#### UI Models

##### BudgetModel
UI-optimized budget representation with calculated display properties:
- Status-based color coding
- Utilization percentage calculations
- Formatted currency displays
- Progress indicators

##### BudgetAlert (UI)
Simplified alert model for UI notifications:
- `BudgetAlertType`: Warning (80% threshold), Exceeded (100% threshold)
- User-friendly messages
- Utilization percentages for progress bars

##### CategoryOption
Category selection model for budget creation:
- Category details (name, color, icon)
- `HasExistingBudget` flag for UI logic

### Service Integration Pattern

The typical integration pattern involves the UI service wrapping the domain service:

```csharp
public class BudgetService : IBudgetService // UI Service
{
    private readonly FinTrack.Shared.Services.IBudgetService _domainBudgetService;
    
    public async Task<IEnumerable<BudgetModel>> GetCurrentMonthBudgetsAsync()
    {
        var domainBudgets = await _domainBudgetService.GetCurrentBudgetsAsync();
        return domainBudgets.Select(MapToBudgetModel);
    }
}
```

## Future Enhancements

### Planned Features
- **Budget Templates**: Reusable budget configurations
- **Multi-Currency Support**: Budget management across different currencies
- **Advanced Analytics**: Trend analysis and predictive modeling
- **Notification Integration**: Push notifications for budget alerts
- **Bulk Operations**: Import/export budget configurations

### API Extensions
- **Filtering**: Advanced filtering options for budget queries
- **Sorting**: Configurable sorting for budget lists
- **Pagination**: Support for large budget collections
- **Aggregations**: Summary statistics and rollup calculations