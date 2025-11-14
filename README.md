# FinTrack

A cross-platform personal finance management application built with .NET MAUI and XAML, featuring offline-first functionality and seamless synchronization across devices.

## 🚀 Features

- **Transaction Management**: Add, edit, and categorize financial transactions with intuitive XAML UI
- **Account Tracking**: Manage multiple accounts with real-time balance tracking
- **Budget Management**: Create and monitor budgets with spending alerts and progress tracking
- **Financial Goals**: Set and track financial goals with milestone visualization
- **Financial Reports**: Generate insights and analytics on spending patterns
- **Offline-First**: Full functionality without internet connectivity
- **Cross-Platform Sync**: Synchronize data across devices when online
- **Feature Flags**: Runtime feature toggling for sync functionality and UI components
- **Dark Theme**: Modern, responsive dark theme optimized for all platforms

## 🎯 Supported Platforms

- **Android** (API 24+)
- **iOS** (15.0+)
- **macOS** (Mac Catalyst 15.0+)
- **Windows** (Windows 10 version 19041+)

## 🏗️ Architecture

FinTrack follows clean architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   XAML Pages    │  │   ViewModels    │  │   Controls  │ │
│  │   & Views       │  │   & Commands    │  │ & Behaviors │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │  App Services   │  │   Sync Logic    │  │ Connectivity│ │
│  │  & Handlers     │  │  & Conflicts    │  │  Monitoring │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │    Entities     │  │   Interfaces    │  │    Enums    │ │
│  │  & Value Objs   │  │ & Repositories  │  │ & Exceptions│ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │ SQLite Database │  │  Repositories   │  │  Platform   │ │
│  │  & EF Core      │  │ & Data Access   │  │  Services   │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Project Structure

```
src/frontend/
├── FinTrack.sln                    # Main solution file
├── src/
│   ├── FinTrack.Maui/             # Main MAUI project with XAML UI
│   ├── FinTrack.Core/             # Domain entities and interfaces
│   ├── FinTrack.Shared/           # Shared business logic
│   └── FinTrack.Infrastructure/   # Data access implementations
└── tests/
    ├── FinTrack.Tests.Unit/       # Unit tests (Core, Shared, Infrastructure)
    └── FinTrack.Tests.Integration/ # Integration tests (includes UI testing)
```

### Project Dependencies

The solution maintains clean dependency boundaries:

```
FinTrack.Maui (Presentation)
├── FinTrack.Shared
├── FinTrack.Core
└── FinTrack.Infrastructure

FinTrack.Shared (Application)
├── FinTrack.Core
└── FinTrack.Infrastructure

FinTrack.Infrastructure (Data)
└── FinTrack.Core

FinTrack.Tests.Unit (Testing)
├── FinTrack.Core
├── FinTrack.Shared
└── FinTrack.Infrastructure
(Note: Does NOT reference FinTrack.Maui to maintain proper test isolation)
```

## 🔧 Technology Stack

- **.NET 10.0** - Target framework for all projects
- **.NET MAUI** - Cross-platform UI framework with XAML
- **Entity Framework Core 10.0** - Data access with SQLite provider
- **SQLite** - Local database for offline-first functionality
- **XAML** - Native UI markup for all platforms
- **Microsoft.Extensions.DependencyInjection** - Service registration and DI
- **Microsoft.Extensions.Logging** - Logging infrastructure
- **CommunityToolkit.Mvvm** - MVVM helpers and commands

## 🚦 Getting Started

### Prerequisites

- .NET 10.0 SDK
- MAUI workloads installed

```bash
# Install MAUI workloads
dotnet workload install maui
```

### Building the Application

```bash
# Clone the repository
git clone <repository-url>
cd fin-track

# Restore packages
dotnet restore src/frontend/FinTrack.sln

# Build entire solution
dotnet build src/frontend/FinTrack.sln

# Build for specific platforms
dotnet build -f net10.0-android
dotnet build -f net10.0-ios
dotnet build -f net10.0-maccatalyst
dotnet build -f net10.0-windows10.0.19041.0
```

### Running the Application

```bash
# Run on Android emulator
dotnet build -t:Run -f net10.0-android

# Run on iOS simulator (macOS only)
dotnet build -t:Run -f net10.0-ios

# Run on Windows
dotnet run --project src/frontend/src/FinTrack.Maui -f net10.0-windows10.0.19041.0
```

### Running Tests

```bash
# Run all tests
dotnet test src/frontend/tests/

# Run unit tests only
dotnet test src/frontend/tests/FinTrack.Tests.Unit/

# Run integration tests only
dotnet test src/frontend/tests/FinTrack.Tests.Integration/
```

## 📱 Key Features

### Offline-First Architecture

FinTrack is designed to work seamlessly offline with automatic synchronization when connectivity is restored:

- **Local SQLite Database**: All data is stored locally for instant access
- **Connectivity Monitoring**: Real-time network status detection via `IConnectivityService`
- **Sync Queue**: Offline changes are queued and synchronized automatically
- **Conflict Resolution**: Smart conflict resolution using timestamp-based "last write wins"

### Connectivity Service

The `IConnectivityService` provides network connectivity detection:

```csharp
public interface IConnectivityService
{
    bool IsConnected { get; }
    event EventHandler<bool> ConnectivityChanged;
    void StartMonitoring();
    void StopMonitoring();
}
```

Features:
- Real-time connectivity status monitoring
- Event-driven connectivity change notifications
- Cross-platform implementation using MAUI's networking APIs
- Automatic service lifecycle management

### Feature Flag Service

The `IFeatureFlagService` enables runtime feature toggling:

```csharp
public interface IFeatureFlagService
{
    bool IsFeatureEnabled(string flagName);
    void SetFeatureFlag(string flagName, bool enabled);
    Dictionary<string, bool> GetAllFeatureFlags();
}
```

Built-in Feature Flags:
- **OfflineSync**: Controls offline synchronization functionality
- **SyncStatusIndicators**: Toggles sync status display in UI
- **AutomaticSync**: Enables/disables automatic background sync
- **ConflictResolution**: Controls conflict resolution dialog availability

### Data Synchronization

- **SyncStatus Enum**: Tracks entity synchronization state (Synced, PendingCreate, PendingUpdate, PendingDelete, SyncFailed, Conflict)
- **SyncOperation Enum**: Defines sync operation types (Create, Update, Delete, None)
- **Automatic Retry**: Failed sync operations are automatically retried
- **Visual Indicators**: UI shows sync status and pending changes

### XAML UI Components

- **AppShell Navigation**: Tab-based navigation with modal support and sync status header
- **Responsive Design**: Grid-based layouts that adapt to different screen sizes
- **Dark Theme**: Consistent #121212 background with modern card-based layouts
- **Touch Optimization**: 44px minimum touch targets with proper spacing
- **Visual Feedback**: Loading indicators, empty states, and offline banners
- **Data Binding**: Two-way binding with ViewModels using MVVM pattern
- **Custom Styling**: Frame-based cards with rounded corners and shadows
- **Accessibility**: Proper semantic markup and screen reader support

## 🗂️ Core Entities

### BaseEntity
All entities inherit from `BaseEntity` which provides:
- `Id`: Primary key
- `CreatedAt` / `UpdatedAt`: Timestamps
- `IsDeleted`: Soft delete flag
- `SyncStatus`: Synchronization status
- `SyncId`: Unique identifier for sync operations

### Domain Entities
- **Transaction**: Financial transactions with amount, description, date, category, and reconciliation support
- **Account**: User accounts with balance tracking, account types, and credit limits
- **Category**: Hierarchical transaction categorization with budget limits and visual styling
- **Goal**: Financial goals with milestone tracking, progress visualization, and priority management
- **GoalMilestone**: Individual milestones within goals with achievement tracking

### Rich Domain Model
The entities include comprehensive business logic and validation:

#### Category Features
- **Hierarchical Structure**: Parent-child category relationships with unlimited depth
- **Visual Styling**: Color codes and icons for visual representation with default fallback (#6B7280)
- **Budget Integration**: Optional monthly budget limits per category
- **Spending Calculations**: Built-in methods for calculating spending with subcategories
- **Validation**: Hex color validation with empty string support, circular reference prevention

#### Goal Management
- **Progress Tracking**: Automatic progress percentage calculation with 100% cap
- **Milestone System**: Multiple milestones per goal with achievement tracking and automatic unlocking
- **Priority Management**: 1-5 priority levels for goal organization and display ordering
- **Smart Calculations**: 
  - **Required Monthly Savings**: Calculates monthly amount needed based on remaining time (returns 0 if overdue or completed)
  - **Days Remaining**: Time until target date with 0 minimum
  - **Overdue Detection**: Automatic detection when target date has passed and goal is incomplete
- **Achievement Logic**: Automatic completion detection and milestone unlocking when progress updates occur
- **Edge Case Handling**: Proper handling of completed goals, overdue goals, and zero-time scenarios

#### Transaction Features
- **Multiple Types**: Income, Expense, and Transfer transaction support
- **Reconciliation**: Bank reconciliation with reconciled date tracking
- **Reference Numbers**: Support for check numbers and transaction references
- **Transfer Handling**: Proper double-entry for account-to-account transfers

## 🔌 Service Architecture

### Repository Pattern
Generic `IRepository<T>` interface provides comprehensive data access:
- **CRUD Operations**: Full async CRUD with cancellation token support
- **Query Operations**: Flexible filtering with LINQ expressions
- **Pagination**: Skip/take pagination with optional ordering
- **Sync Operations**: GetPendingSyncAsync, MarkAsSyncedAsync, GetBySyncStatusAsync
- **Soft Delete**: IsDeleted flag with proper filtering
- **Bulk Operations**: AddRangeAsync, UpdateRangeAsync for performance
- **Specialized Repositories**: IGoalRepository with goal-specific operations like GetGoalsByPriorityAsync

### Application Services
- **TransactionService**: Business logic for transaction management
- **BudgetService**: Budget creation and monitoring
- **GoalService**: Financial goal tracking
- **SyncService**: Data synchronization coordination
- **ConnectivityService**: Network connectivity monitoring
- **FeatureFlagService**: Runtime feature flag management and toggling

## 🧪 Testing

The project includes comprehensive testing with proper isolation and dedicated test utilities:

### Unit Tests (FinTrack.Tests.Unit)
- **Domain Logic**: Entity validation, business rules, and domain operations
- **Services**: Application service logic with mocked dependencies
- **Repositories**: Data access patterns with in-memory databases and comprehensive interface contract testing
- **Repository Interface**: Complete `IRepository<T>` contract verification with 22 test methods covering CRUD, querying, pagination, and sync operations
- **Sync Logic**: Offline synchronization and conflict resolution with dedicated test helpers
- **Dependencies**: References Core, Shared, and Infrastructure projects only
- **Isolation**: Does NOT reference FinTrack.Maui to maintain clean separation

### Test Utilities & Helpers
- **TestDataBuilder**: Fluent API for creating test data objects with builder pattern
  - `TransactionBuilder`: Creates test transactions with configurable properties
  - `AccountBuilder`: Creates test accounts with various configurations
  - `CategoryBuilder`: Creates test categories with hierarchical support
  - `GoalBuilder`: Creates test financial goals with milestones
- **TestScenarios**: Pre-configured common test scenarios
  - `TypicalCheckingAccount()`: Standard checking account with transactions
  - `CreditCardWithDebt()`: Credit card account with negative balance
  - `EmergencyFundGoal()`: Sample emergency fund goal with milestones
- **Type Safety**: Test helpers use actual Core entities and interfaces
- **Consistency**: Ensures test objects match production domain model contracts

### Integration Tests (FinTrack.Tests.Integration)
- **Database Operations**: Real SQLite database integration testing
- **Sync Functionality**: End-to-end synchronization scenarios
- **UI Testing**: XAML page navigation and user interactions
- **Platform Services**: Platform-specific service implementations
- **Cross-Platform**: Tests across Android, iOS, Windows, and macOS

### Test Architecture Principles
- **Dependency Isolation**: Unit tests avoid UI layer dependencies
- **Fast Execution**: Unit tests run quickly without UI overhead
- **Mocking**: Use Moq for service and repository mocking
- **In-Memory Testing**: Use in-memory databases for repository tests
- **Contract Testing**: Comprehensive `IRepository<T>` interface contract verification ensures consistent behavior across all implementations
- **Platform Testing**: Integration tests verify platform-specific behavior
- **Interface Consistency**: Test helpers reference actual Core interfaces for type safety
- **Type Safety**: Proper use of C# type system with correct decimal literals and type-safe test data
- **Code Quality**: Consistent coding standards applied to test code for maintainability

### Testing Best Practices
- **Type Matching**: Always match test parameter types with actual property types (e.g., use `decimal?` for testing `decimal?` properties)
- **Decimal Literals**: Use explicit decimal literals with `m` suffix (`100m`, `0.5m`) when testing decimal properties
- **Theory Data**: Ensure xUnit `[InlineData]` values match the test method parameter types exactly
- **Explicit Casting**: Avoid implicit type conversions in test data that may introduce precision issues
- **Property Validation**: Test boundary conditions using the same data types as the production code
- **Type Safety**: Recent improvements include using `.0` notation for numeric literals, but full type safety requires matching parameter types with property types

## 🚀 Development Workflow

### Code Style & Conventions
- **C# 13** language features preferred (with .NET 10.0)
- **Async/await** for all I/O operations
- **CancellationToken** parameters for async methods
- **Nullable reference types** enabled
- **File-scoped namespaces** for new files

### Dependency Injection
Services are registered in `MauiProgram.cs` with appropriate lifetimes:
- **Singleton**: Stateless services (ConnectivityService, SyncService, FeatureFlagService)
- **Transient**: ViewModels and Pages
- **Scoped**: Database contexts and repositories

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 Recent Changes

### Test Code Type Safety Improvement (Latest)
- **Decimal Literal Fix**: Updated test data in `CategoryEntityTests.cs` to use explicit decimal notation (`.0` suffix) for better type clarity
- **Issue Identified**: The `IsValid_WithDifferentBudgetLimits_ShouldReturnExpectedResult` test method has a type mismatch where parameter type is `double?` but `Category.BudgetLimit` property is `decimal?`
- **Current Workaround**: Test uses explicit casting `(decimal)budgetLimit.Value` to convert from double to decimal
- **Recommended Fix**: Change test method parameter from `double? budgetLimit` to `decimal? budgetLimit` and use decimal literals (`-100m`, `0m`, `100m`) instead of double literals
- **Impact**: Current implementation works but requires unnecessary casting and may introduce precision issues
- **Best Practice**: Always match test parameter types with actual property types being tested to ensure type safety and avoid casting

### Goal Entity RequiredMonthlySavings Logic Update
- **Calculation Logic Refinement**: Updated `RequiredMonthlySavings` calculation to return 0 when goal is completed or overdue (DaysRemaining <= 0)
- **Test Accuracy**: Fixed unit tests in `GoalEntityTests.cs` to match actual implementation behavior
- **Edge Case Handling**: Improved handling of scenarios where target date has passed or no time remains
- **Business Logic**: When DaysRemaining is 0 or negative, the calculation returns 0 instead of the full remaining amount
- **Monthly Calculation**: For active goals, uses `Math.Max(DaysRemaining / 30.0m, 1)` to ensure minimum 1-month calculation period

### Category Validation Enhancement
- **Color Validation Update**: Empty string colors are now considered valid in Category entities (uses default #6B7280 color)
- **Test Coverage**: Updated `CategoryTests.cs` to reflect that empty color strings are acceptable
- **Domain Logic**: Category validation now properly handles null/empty colors by falling back to default styling
- **Backward Compatibility**: Existing categories with empty colors remain valid and functional

### Repository Interface Testing
- **Comprehensive Contract Testing**: Added `IRepositoryTests.cs` with 22 test methods covering all `IRepository<T>` interface operations
- **CRUD Operation Testing**: Complete coverage of Create, Read, Update, Delete operations with both single and bulk variants
- **Query Operation Testing**: Verification of filtering, pagination, counting, and existence checking methods
- **Sync Operation Testing**: Full testing of sync-related methods including pending sync detection and status management
- **Mock-Based Approach**: Uses Moq framework for behavior verification and contract compliance
- **Test Entity**: Dedicated `TestEntity` class for isolated repository interface testing

### .NET 10.0 Migration
- **Updated Target Framework**: All projects now target .NET 10.0
- **Enhanced Performance**: Leveraging latest .NET runtime optimizations
- **Updated Dependencies**: Entity Framework Core 10.0 and related packages
- **Build Commands**: Updated to use net10.0-* target framework monikers
- **C# 13 Support**: Access to latest C# language features
- **Project File Cleanup**: Removed BOM (Byte Order Mark) from project files for consistency

### XAML UI Implementation (Completed)
- **Pure XAML MAUI**: Converted from Blazor Hybrid to native XAML implementation
- **AppShell Navigation**: Tab-based navigation with sync status indicators
- **Dashboard Page**: Financial overview with summary cards and recent transactions
- **Transactions Page**: Full transaction management with search, filtering, and CRUD operations
- **Feature Flags Page**: Runtime feature toggling interface
- **Offline Support**: Visual offline indicators and connectivity monitoring
- **Dark Theme**: Consistent modern dark theme across all pages

### Architecture Improvements
- **Feature Flag Service**: Runtime feature toggling for sync functionality
- **Connectivity Service**: Real-time network monitoring across platforms
- **Sync Status UI**: Visual indicators for offline/online status in AppShell
- **Rich Domain Model**: Comprehensive entity validation and business logic
- **Test Infrastructure**: Fluent test data builders and comprehensive test scenarios

## 📞 Support

For support and questions, please open an issue in the GitHub repository.