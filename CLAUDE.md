# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FinTrack is a cross-platform, offline-first personal finance management app built with **.NET MAUI** and **XAML** (no Blazor). It targets Android, iOS, macOS (Mac Catalyst), and Windows, and stores data locally in SQLite with sync-status tracking for eventual cross-device synchronization.

All application code currently lives under `src/frontend/`. A `src/backend/` (planned .NET API / Python services) does not exist yet — don't assume it does.

## Commands

All commands are run from the repo root unless noted; the solution file is `src/frontend/FinTrack.sln`.

```bash
# Restore / build / clean the whole solution
dotnet restore src/frontend/FinTrack.sln
dotnet build src/frontend/FinTrack.sln
dotnet clean src/frontend/FinTrack.sln

# Build for a specific platform (MAUI multi-targets)
dotnet build -f net10.0-android
dotnet build -f net10.0-ios
dotnet build -f net10.0-maccatalyst
dotnet build -f net10.0-windows10.0.19041.0

# Run
dotnet build -t:Run -f net10.0-android      # Android emulator
dotnet build -t:Run -f net10.0-ios          # iOS simulator (macOS only)
dotnet run --project src/frontend/src/FinTrack.Maui -f net10.0-windows10.0.19041.0

# Tests
dotnet test src/frontend/tests/                                    # everything
dotnet test src/frontend/tests/FinTrack.Tests.Unit/                # unit only
dotnet test src/frontend/tests/FinTrack.Tests.Integration/         # integration only

# Single test (xUnit filter, works with FullyQualifiedName or DisplayName substrings)
dotnet test src/frontend/tests/FinTrack.Tests.Unit/ --filter "FullyQualifiedName~GoalEntityTests"
dotnet test src/frontend/tests/FinTrack.Tests.Unit/ --filter "FullyQualifiedName~GoalEntityTests.RequiredMonthlySavings_WhenOverdue_ReturnsZero"
```

Prerequisites: .NET 10.0 SDK and the MAUI workload (`dotnet workload install maui`). Only `FinTrack.Core`, `FinTrack.Shared`, `FinTrack.Infrastructure`, and both test projects build on Linux/CI — `FinTrack.Maui` requires platform workloads and is excluded from the Linux CI job (see `.github/workflows/ci.yml`).

## Architecture

### Layering and dependency direction

Clean-architecture style, one-way dependencies only:

```
FinTrack.Maui (Presentation: XAML Views, ViewModels, platform Services)
  → FinTrack.Shared (Application: TransactionService, BudgetService, GoalService, SyncService, FeatureFlagService)
  → FinTrack.Core (Domain: Entities, Interfaces, Enums, ValueObjects, Exceptions — no dependencies)
  → FinTrack.Infrastructure (Data: EF Core DbContext, repository implementations, SQLite)
```

`FinTrack.Tests.Unit` references Core/Shared/Infrastructure **only** — it deliberately does not reference `FinTrack.Maui`, so unit tests stay UI-free and fast. `FinTrack.Tests.Integration` covers database, sync, and platform/UI interaction scenarios.

### Domain model

- Every entity inherits `BaseEntity` (`Id`, `CreatedAt`/`UpdatedAt`, `IsDeleted` soft-delete flag, `SyncStatus`, `SyncId`).
- Core entities: `Transaction` (Income/Expense/Transfer, reconciliation, references), `Account` (balance, credit limit), `Category` (hierarchical parent/child, hex color with `#6B7280` fallback, optional budget limit), `Goal` + `GoalMilestone` (progress %, priority 1–5, `RequiredMonthlySavings`/`DaysRemaining` calculations that return 0 once overdue or complete).
- Data access goes through a generic `IRepository<T>` (CRUD, LINQ predicate queries, paging, soft delete, bulk ops, plus sync-specific methods: `GetPendingSyncAsync`, `MarkAsSyncedAsync`, `GetBySyncStatusAsync`). `IGoalRepository` adds goal-specific queries on top. `IRepositoryTests.cs` in the unit test project is a full contract test suite (22 methods) that any new repository implementation is expected to satisfy.

### Offline-first sync

- `SyncStatus` enum (Synced, PendingCreate, PendingUpdate, PendingDelete, SyncFailed, Conflict) and `SyncOperation` enum drive an offline queue with automatic retry and timestamp-based "last write wins" conflict resolution.
- `IConnectivityService` monitors network state and raises `ConnectivityChanged`; `SyncService` coordinates synchronization; sync status is surfaced in the AppShell header.
- `IFeatureFlagService` gates sync-related behavior at runtime (`OfflineSync`, `SyncStatusIndicators`, `AutomaticSync`, `ConflictResolution` flags) — check this before assuming sync UI/logic is always active.

### MAUI project layout

`FinTrack.Maui/`: `Views/` (XAML pages), `ViewModels/` (MVVM, CommunityToolkit.Mvvm-based, `BaseViewModel` root), `Services/` (UI-facing services), `Converters/`, `Behaviors/`, `Controls/`, `Platforms/{Android,iOS,MacCatalyst,Windows}/` for platform-specific code (`#if ANDROID` etc.), and `MauiProgram.cs` for DI registration. Dark theme (#121212 background) throughout, Shell-based tab navigation with modal support.

Dependency injection lifetimes registered in `MauiProgram.cs`: singleton for stateless services (Connectivity, Sync, FeatureFlag), transient for ViewModels/Pages, scoped for the DbContext and repositories.

### Test conventions

- `FinTrack.Tests.Unit/Helpers/TestDataBuilder.cs` provides fluent builders (`TestDataBuilder.Transaction()`, `.Account()`, `.Category()`, `.Goal()`) plus `TestScenarios` presets (`TypicalCheckingAccount()`, `CreditCardWithDebt()`, `EmergencyFundGoal()`, etc.) — prefer these over hand-rolling entities in new tests.
- Repository tests use Moq + EF Core in-memory provider; keep new repository tests aligned with the `IRepositoryTests` contract style.
- **Match test parameter types to the actual property type being tested** — e.g. use `decimal?`/`m`-suffixed literals for anything backing a `decimal?` property (like `Category.BudgetLimit`), not `double`. A known outstanding issue (`docs/known-issues.md`) is exactly this mismatch in `CategoryEntityTests.IsValid_WithDifferentBudgetLimits_ShouldReturnExpectedResult` — don't replicate that pattern in new tests.

## Code style

- C# 13 / .NET 10.0, nullable reference types enabled, file-scoped namespaces for new files, async/await with `CancellationToken` parameters for all I/O.
