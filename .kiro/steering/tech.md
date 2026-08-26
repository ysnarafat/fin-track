# Technology Stack & Build System

## Core Technologies

- **.NET 10.0 LTS** - Target framework for all projects
- **.NET MAUI** - Cross-platform UI framework with XAML
- **Entity Framework Core** - Data access with SQLite provider
- **SQLite** - Local database for offline-first functionality
- **XAML** - Native UI markup for all platforms (no Blazor)

## Project Structure

```
src/
├── frontend/
│   ├── FinTrack.sln                    # Main solution file
│   ├── src/
│   │   ├── FinTrack.Maui/             # Main MAUI project
│   │   ├── FinTrack.Core/             # Domain entities and interfaces
│   │   ├── FinTrack.Shared/           # Shared business logic
│   │   └── FinTrack.Infrastructure/   # Data access implementations
│   └── tests/
│       ├── FinTrack.Tests.Unit/       # Unit tests
│       └── FinTrack.Tests.Integration/ # Integration tests
└── backend/
    ├── dotnet/                        # Future .NET API
    └── python/                        # Future Python services
```

## Dependencies & Frameworks

- **Microsoft.Extensions.DependencyInjection** - Service registration and DI
- **Microsoft.Extensions.Logging** - Logging infrastructure
- **CommunityToolkit.Mvvm** - MVVM helpers and commands (recommended)
- **SQLite-net-pcl** or **Microsoft.EntityFrameworkCore.Sqlite** - Database access

## Build Commands

### Prerequisites
```bash
# Install .NET 10.0 LTS SDK
# Install MAUI workloads
dotnet workload install maui
```

### Common Commands
```bash
# Build entire solution
dotnet build src/frontend/FinTrack.sln

# Build for specific platforms
dotnet build -f net10.0-android
dotnet build -f net10.0-ios
dotnet build -f net10.0-maccatalyst
dotnet build -f net10.0-windows10.0.19041.0

# Run tests
dotnet test src/frontend/tests/

# Clean solution
dotnet clean src/frontend/FinTrack.sln

# Restore packages
dotnet restore src/frontend/FinTrack.sln
```

### Development Workflow
```bash
# Run on Android emulator
dotnet build -t:Run -f net10.0-android

# Run on iOS simulator (macOS only)
dotnet build -t:Run -f net10.0-ios

# Run on Windows
dotnet run --project src/frontend/src/FinTrack.Maui -f net10.0-windows10.0.19041.0
```

## Code Style & Conventions

- **C# 13** language features preferred
- **Async/await** for all I/O operations
- **CancellationToken** parameters for async methods
- **Nullable reference types** enabled
- **File-scoped namespaces** for new files
- **Primary constructors** where appropriate

## Platform-Specific Notes

- Use `#if ANDROID`, `#if IOS`, etc. for platform-specific code
- Platform-specific implementations go in `Platforms/` folders
- Dependency injection for platform services
- Handler customizations for platform-specific UI behavior