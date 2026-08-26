# Testing Strategy

## Overview

FinTrack employs a comprehensive testing strategy with clear separation between unit tests, integration tests, and dedicated test utilities. The testing architecture emphasizes proper isolation, fast execution, and type safety through the use of actual Core interfaces in test helpers.

## Test Project Structure

### FinTrack.Tests.Unit
Contains fast-running unit tests with no external dependencies:

```
FinTrack.Tests.Unit/
├── Domain/                     # Entity and domain logic tests
│   ├── Entities/               # Domain entity tests
│   └── ValueObjects/           # Value object tests (Money, DateRange, SyncMetadata)
├── Services/                   # Application service tests
├── Repositories/               # Repository pattern tests
│   └── IRepositoryTests.cs     # Generic repository interface contract tests
├── Sync/                       # Synchronization logic tests
└── Helpers/                    # Test utility classes
    ├── TestDataBuilder.cs      # Fluent test data builders
    └── SyncTestHelpers.cs      # Sync-specific test utilities
```

### FinTrack.Tests.Integration
Contains integration tests that verify component interactions:

```
FinTrack.Tests.Integration/
├── Database/                   # SQLite integration tests
├── Sync/                       # End-to-end sync scenarios
├── Platform/                   # Platform-specific service tests
└── UI/                         # XAML navigation and interaction tests
```

## Test Utilities and Helpers

### TestDataBuilder
The `TestDataBuilder` class provides a fluent API for creating test data objects:

```csharp
public static class TestDataBuilder
{
    public static TransactionBuilder Transaction() => new TransactionBuilder();
    public static AccountBuilder Account() => new AccountBuilder();
    public static CategoryBuilder Category() => new CategoryBuilder();
    public static GoalBuilder Goal() => new GoalBuilder();
}
```

**Builder Classes:**
- **TransactionBuilder**: Creates transactions with configurable properties
- **AccountBuilder**: Creates accounts with various configurations  
- **CategoryBuilder**: Creates categories with hierarchical support
- **GoalBuilder**: Creates financial goals with milestones

**Key Features:**
- **Fluent API**: Chainable methods for readable test setup
- **Type Safety**: Uses actual `FinTrack.Core.Entities` types
- **Flexibility**: Supports complex entity configurations
- **Maintainability**: Changes to entities automatically propagate to tests

### TestScenarios
Pre-configured common test scenarios for typical use cases:

```csharp
public static class TestScenarios
{
    public static Account TypicalCheckingAccount();
    public static Account CreditCardWithDebt();
    public static Transaction TypicalExpenseTransaction();
    public static Transaction TypicalIncomeTransaction();
    public static Transaction TypicalTransferTransaction();
    public static Category FoodCategory();
    public static Goal EmergencyFundGoal();
}
```

### Usage Examples

```csharp
// Using builders for custom test data
var transaction = TestDataBuilder.Transaction()
    .WithAmount(85.50m)
    .WithDescription("Grocery Shopping")
    .WithType(TransactionType.Expense)
    .WithDate(DateTime.Today)
    .AsReconciled()
    .Build();

// Using pre-configured scenarios
var account = TestScenarios.TypicalCheckingAccount();
var goal = TestScenarios.EmergencyFundGoal();
```

## Testing Principles

### Dependency Isolation
- Unit tests reference only Core, Shared, and Infrastructure projects
- No dependencies on FinTrack.Maui to maintain clean separation
- Use of interfaces and dependency injection for easy mocking

### Fast Execution
- Unit tests run in milliseconds without external dependencies
- In-memory databases for repository testing
- Mocked services for business logic testing

### Type Safety
- Test helpers use actual production interfaces
- Compile-time verification of test object compatibility
- Automatic detection of interface changes through build failures
- Proper use of C# type system with correct decimal literals and type-safe test data
- Consistent coding standards applied to test code for maintainability

#### Type Safety Best Practices
- **Parameter Type Matching**: Always match test method parameter types with the actual property types being tested
- **Decimal Literal Usage**: Use explicit decimal literals with `m` suffix (`100m`, `0.5m`, `-100m`) when testing decimal properties
- **Theory Data Consistency**: Ensure xUnit `[InlineData]` attribute values match the test method parameter types exactly
- **Avoid Implicit Conversions**: Prevent implicit type conversions in test data that may introduce precision issues or runtime errors
- **Property Type Verification**: Verify that test data types align with domain entity property types before writing tests

#### Common Type Safety Issues
- **Decimal vs Double**: Using `double` values when testing `decimal?` properties requires explicit casting and may cause precision loss
- **Nullable Type Handling**: Properly handle nullable types in test data to match entity property definitions
- **Literal Suffix Requirements**: C# requires explicit suffixes for decimal literals to prevent ambiguous type inference

### Comprehensive Coverage
- **Domain Logic**: Entity validation, business rules, and domain operations
- **Value Objects**: Money arithmetic, DateRange validation, SyncMetadata state management
- **Repository Patterns**: Data access patterns with in-memory databases and interface contract verification
- **Repository Interface**: Complete `IRepository<T>` contract testing with 22 test methods covering CRUD, querying, pagination, and sync operations
- **Synchronization**: Sync scenarios, conflict resolution, and retry logic
- **Platform Services**: Platform-specific service implementations
- **UI Navigation**: XAML page navigation and user interactions

### Entity Testing
The test suite includes comprehensive coverage for all domain entities:

#### Transaction Tests
- Amount validation and type checking
- Date range validation and business rules
- Transaction type handling (Income, Expense, Transfer)
- Reconciliation logic and state management
- Reference number and notes handling

#### Account Tests
- Balance calculation and tracking
- Account type validation (Checking, Savings, Credit Card)
- Credit limit enforcement for credit accounts
- Transaction relationship management
- Currency validation and consistency

#### Category Tests
- Hierarchical structure validation
- Color and icon validation (hex color format with empty string support)
- Budget limit enforcement and calculations with proper decimal literal usage
- Spending calculation with subcategories
- Circular reference prevention
- Default color fallback behavior (#6B7280 for null/empty colors)
- Type-safe test data using appropriate C# decimal literals for budget limits

**Recent Update**: The `IsValid_WithDifferentBudgetLimits_ShouldReturnExpectedResult` test method was partially improved by adding `.0` suffixes to numeric literals for better type clarity. However, a type mismatch still exists where the parameter type is `double?` but the `Category.BudgetLimit` property is `decimal?`. This requires explicit casting and may introduce precision issues. The test should be fully updated to use `decimal?` parameter type with proper decimal literals (`-100m`, `0m`, `100m`).

#### Goal Tests
- Progress calculation and milestone tracking with percentage capping at 100%
- Priority management and sorting for goal organization
- Target date validation and overdue detection logic
- Required monthly savings calculation with edge case handling:
  - Returns 0 for completed goals
  - Returns 0 for overdue goals (DaysRemaining <= 0)
  - Calculates monthly amount based on remaining time for active goals
- Milestone achievement logic and automatic unlocking
- Goal completion detection and timestamp tracking
- Business logic validation for various goal states

## Repository Interface Testing

### IRepositoryTests
The `IRepositoryTests` class provides comprehensive contract testing for the generic `IRepository<T>` interface, ensuring all repository implementations follow consistent behavior patterns.

#### Test Coverage (22 Test Methods)

**Basic CRUD Operations:**
- `GetByIdAsync_WithValidId_ShouldReturnEntity`
- `GetByIdAsync_WithInvalidId_ShouldReturnNull`
- `GetBySyncIdAsync_WithValidSyncId_ShouldReturnEntity`
- `AddAsync_WithValidEntity_ShouldReturnEntityWithId`
- `UpdateAsync_WithValidEntity_ShouldReturnUpdatedEntity`
- `DeleteAsync_WithValidId_ShouldReturnTrue`
- `DeleteAsync_WithInvalidId_ShouldReturnFalse`
- `DeleteAsync_WithEntity_ShouldReturnTrue`
- `HardDeleteAsync_WithValidId_ShouldReturnTrue`

**Bulk Operations:**
- `AddRangeAsync_WithMultipleEntities_ShouldReturnAllEntitiesWithIds`
- `UpdateRangeAsync_WithMultipleEntities_ShouldReturnUpdatedEntities`

**Query Operations:**
- `GetAllAsync_ShouldReturnAllNonDeletedEntities`
- `GetWhereAsync_WithPredicate_ShouldReturnMatchingEntities`
- `GetSingleAsync_WithMatchingPredicate_ShouldReturnEntity`
- `CountAsync_WithoutPredicate_ShouldReturnTotalCount`
- `CountAsync_WithPredicate_ShouldReturnFilteredCount`
- `AnyAsync_WithMatchingPredicate_ShouldReturnTrue`
- `GetPagedAsync_WithPagination_ShouldReturnCorrectPage`

**Sync-Related Operations:**
- `GetPendingSyncAsync_ShouldReturnEntitiesNeedingSync`
- `GetBySyncStatusAsync_WithSpecificStatus_ShouldReturnMatchingEntities`
- `GetModifiedAfterAsync_WithTimestamp_ShouldReturnRecentlyModifiedEntities`
- `MarkAsSyncedAsync_WithSyncIds_ShouldReturnUpdatedCount`
- `MarkAsConflictedAsync_WithSyncIds_ShouldReturnUpdatedCount`

#### Test Entity
Uses a dedicated `TestEntity` class that inherits from `BaseEntity`:

```csharp
public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
```

#### Testing Approach
- **Mock-Based Testing**: Uses Moq framework for repository mocking
- **Behavior Verification**: Verifies method calls and return values
- **Contract Compliance**: Ensures all repository implementations follow the same interface contract
- **Sync Integration**: Tests offline sync functionality and status management
- **Cancellation Token Support**: Verifies proper async/await patterns with cancellation tokens

#### Benefits
- **Interface Consistency**: Guarantees all repository implementations behave identically
- **Regression Prevention**: Catches breaking changes to repository contracts
- **Documentation**: Serves as living documentation of expected repository behavior
- **Sync Reliability**: Ensures offline sync operations work correctly across all repositories