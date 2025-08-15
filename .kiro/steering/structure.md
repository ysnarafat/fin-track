# Project Organization & Structure

## Solution Architecture

The FinTrack solution follows clean architecture principles with clear separation of concerns across multiple projects.

### Project Dependencies

```
FinTrack.Maui (UI Layer)
├── FinTrack.Shared (Application Layer)
├── FinTrack.Core (Domain Layer)
└── FinTrack.Infrastructure (Infrastructure Layer)

FinTrack.Shared
├── FinTrack.Core
└── FinTrack.Infrastructure

FinTrack.Infrastructure
└── FinTrack.Core
```

## Folder Structure Conventions

### FinTrack.Maui (Main MAUI Project)
```
FinTrack.Maui/
├── Platforms/                  # Platform-specific implementations
│   ├── Android/
│   ├── iOS/
│   ├── MacCatalyst/
│   └── Windows/
├── Resources/                  # App resources
│   ├── Fonts/
│   ├── Images/
│   └── Styles/
├── Views/                      # XAML pages and views
│   ├── DashboardPage.xaml
│   ├── TransactionsPage.xaml
│   ├── TransactionFormPage.xaml
│   ├── AccountsPage.xaml
│   ├── ReportsPage.xaml
│   └── BudgetsPage.xaml
├── ViewModels/                 # MVVM ViewModels
│   ├── BaseViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── TransactionsViewModel.cs
│   └── TransactionFormViewModel.cs
├── Services/                   # UI-specific services
├── Controls/                   # Custom XAML controls
├── Converters/                 # Value converters
├── Behaviors/                  # XAML behaviors
├── AppShell.xaml              # Navigation shell
├── App.xaml                   # Application definition
└── MauiProgram.cs             # App configuration
```

### FinTrack.Core (Domain Layer)
```
FinTrack.Core/
├── Entities/                   # Domain entities
│   ├── BaseEntity.cs
│   ├── Transaction.cs
│   ├── Account.cs
│   ├── Category.cs
│   └── Budget.cs
├── Enums/                      # Domain enumerations
│   ├── TransactionType.cs
│   ├── AccountType.cs
│   ├── SyncStatus.cs
│   └── SyncOperation.cs
├── Interfaces/                 # Repository and service contracts
│   ├── IRepository.cs
│   ├── ITransactionRepository.cs
│   ├── IAccountRepository.cs
│   └── ISyncService.cs
├── ValueObjects/               # Domain value objects
└── Exceptions/                 # Domain-specific exceptions
```

### FinTrack.Shared (Application Layer)
```
FinTrack.Shared/
├── Services/                   # Application services
│   ├── TransactionService.cs
│   ├── BudgetService.cs
│   ├── SyncService.cs
│   └── ReportService.cs
├── DTOs/                       # Data transfer objects
├── Mappers/                    # Entity to DTO mapping
├── Validators/                 # Business rule validation
├── Commands/                   # Command pattern implementations
├── Queries/                    # Query pattern implementations
└── Extensions/                 # Utility extensions
```

### FinTrack.Infrastructure (Infrastructure Layer)
```
FinTrack.Infrastructure/
├── Data/                       # Database context and configuration
│   ├── FinTrackDbContext.cs
│   ├── Configurations/         # Entity configurations
│   └── Migrations/             # EF Core migrations
├── Repositories/               # Repository implementations
│   ├── BaseRepository.cs
│   ├── TransactionRepository.cs
│   └── AccountRepository.cs
├── Services/                   # External service implementations
│   ├── ApiService.cs
│   ├── FileService.cs
│   └── NotificationService.cs
├── Sync/                       # Synchronization logic
└── Platform/                   # Platform-specific services
```

### Test Projects
```
tests/
├── FinTrack.Tests.Unit/
│   ├── Services/               # Service layer tests
│   ├── Repositories/           # Repository tests
│   ├── Domain/                 # Domain logic tests
│   └── Helpers/                # Test utilities
└── FinTrack.Tests.Integration/
    ├── Database/               # Database integration tests
    ├── Sync/                   # Sync functionality tests
    └── Platform/               # Platform service tests
```

## Naming Conventions

### Files and Classes
- **Pages**: `{Feature}Page.xaml` (e.g., `TransactionsPage.xaml`)
- **ViewModels**: `{Feature}ViewModel.cs` (e.g., `TransactionsViewModel.cs`)
- **Services**: `{Domain}Service.cs` (e.g., `TransactionService.cs`)
- **Repositories**: `{Entity}Repository.cs` (e.g., `TransactionRepository.cs`)
- **Entities**: Singular nouns (e.g., `Transaction.cs`, `Account.cs`)
- **Interfaces**: `I{Name}` prefix (e.g., `ITransactionService`)

### Namespaces
- **FinTrack.Maui.Views** - XAML pages and views
- **FinTrack.Maui.ViewModels** - ViewModels
- **FinTrack.Core.Entities** - Domain entities
- **FinTrack.Core.Interfaces** - Repository and service contracts
- **FinTrack.Shared.Services** - Application services
- **FinTrack.Infrastructure.Data** - Database context and configurations

## Code Organization Rules

### Dependency Injection Registration
- Register services in `MauiProgram.cs`
- Use appropriate lifetimes: Singleton for stateless services, Transient for ViewModels
- Group registrations by layer (Services, ViewModels, Pages)

### XAML Organization
- Use consistent indentation (4 spaces)
- Group related properties together
- Use data binding over code-behind
- Implement proper MVVM patterns

### Entity Relationships
- All entities inherit from `BaseEntity`
- Use navigation properties for relationships
- Implement proper foreign key constraints
- Include sync-related properties for offline functionality

### Error Handling
- Use custom exceptions in domain layer
- Implement global exception handling in MAUI app
- Log errors appropriately for debugging
- Provide user-friendly error messages in UI