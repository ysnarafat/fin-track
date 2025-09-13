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

- **.NET 8.0** - Target framework
- **.NET MAUI** - Cross-platform UI framework with XAML
- **Entity Framework Core** - Data access with SQLite provider
- **SQLite** - Local database for offline-first functionality
- **XAML** - Native UI markup for all platforms
- **Microsoft.Extensions.DependencyInjection** - Service registration and DI
- **Microsoft.Extensions.Logging** - Logging infrastructure

## 🚦 Getting Started

### Prerequisites

- .NET 8.0 SDK
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
dotnet build -f net8.0-android
dotnet build -f net8.0-ios
dotnet build -f net8.0-maccatalyst
dotnet build -f net8.0-windows10.0.19041.0
```

### Running the Application

```bash
# Run on Android emulator
dotnet build -t:Run -f net8.0-android

# Run on iOS simulator (macOS only)
dotnet build -t:Run -f net8.0-ios

# Run on Windows
dotnet run --project src/frontend/src/FinTrack.Maui -f net8.0-windows10.0.19041.0
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

- **AppShell Navigation**: Tab-based navigation with modal support
- **Responsive Design**: Adapts to different screen sizes and orientations
- **Dark Theme**: Consistent dark theme across all platforms
- **Touch Optimization**: 44px minimum touch targets for mobile usability
- **Accessibility**: WCAG 2.1 AA compliance features

## 🗂️ Core Entities

### BaseEntity
All entities inherit from `BaseEntity` which provides:
- `Id`: Primary key
- `CreatedAt` / `UpdatedAt`: Timestamps
- `IsDeleted`: Soft delete flag
- `SyncStatus`: Synchronization status
- `SyncId`: Unique identifier for sync operations

### Domain Entities
- **Transaction**: Financial transactions with amount, description, date, and category
- **Account**: User accounts with balance tracking and account type
- **Category**: Transaction categorization system with hierarchical support
- **Goal**: Financial goals with milestone tracking and progress visualization

### Value Objects
The domain includes several value objects for type safety and business logic encapsulation:

#### Money
Represents monetary amounts with currency validation:
```csharp
var price = new Money(100.50m, "USD");
var total = price + new Money(25.00m, "USD");
```
- **Currency Validation**: Enforces 3-letter ISO currency codes
- **Arithmetic Operations**: Safe addition, subtraction, multiplication, and division
- **Currency Consistency**: Prevents operations between different currencies
- **Utility Methods**: `Abs()`, `Negate()`, `IsPositive`, `IsNegative`, `IsZero`

#### DateRange
Represents date ranges with validation and utility methods:
```csharp
var currentMonth = DateRange.CurrentMonth();
var lastWeek = DateRange.LastDays(7);
var isInRange = currentMonth.Contains(DateTime.Today);
```
- **Factory Methods**: `CurrentMonth()`, `CurrentYear()`, `LastDays()`, `ForMonth()`
- **Validation**: Ensures start date is not after end date
- **Utility Methods**: `Contains()`, `OverlapsWith()`, `DayCount`

#### SyncMetadata
Encapsulates synchronization state and metadata:
```csharp
var metadata = SyncMetadata.CreateNew(deviceId);
var updated = metadata.MarkAsModified(deviceId);
```
- **State Management**: Tracks sync status, version, and retry attempts
- **Conflict Resolution**: Supports optimistic concurrency control
- **Error Tracking**: Records sync failures and retry counts

## 🔌 Service Architecture

### Repository Pattern
Generic `IRepository<T>` interface provides:
- Basic CRUD operations with async/await
- Pagination and filtering support
- Sync-specific operations (GetPendingSyncAsync, MarkAsSyncedAsync)
- Soft delete functionality

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
- **Repositories**: Data access patterns with in-memory databases
- **Sync Logic**: Offline synchronization and conflict resolution with dedicated test helpers
- **Dependencies**: References Core, Shared, and Infrastructure projects only
- **Isolation**: Does NOT reference FinTrack.Maui to maintain clean separation

### Test Utilities & Helpers
- **SyncTestHelpers**: Utility class for creating sync-related test objects
  - `CreateSyncStateChangedEventArgs()`: Creates test sync state change events
  - `CreateSyncConflict()`: Creates test sync conflicts for resolution testing
- **Value Object Testing**: Comprehensive tests for Money, DateRange, and SyncMetadata value objects
- **Type Safety**: Test helpers use actual Core interfaces rather than duplicating types
- **Consistency**: Ensures test objects match production interface contracts

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
- **Platform Testing**: Integration tests verify platform-specific behavior
- **Interface Consistency**: Test helpers reference actual Core interfaces for type safety

## 🚀 Development Workflow

### Code Style & Conventions
- **C# 12** language features preferred
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

## 📞 Support

For support and questions, please open an issue in the GitHub repository.
