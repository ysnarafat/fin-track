using FinTrack.Infrastructure.Data;
using FinTrack.Maui.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Configure Entity Framework
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "fintrack.db");
        builder.Services.AddDbContext<FinTrackDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}")
#if DEBUG
                   .EnableSensitiveDataLogging()
                   .EnableDetailedErrors()
#endif
                   );

        // Register Infrastructure Services
        builder.Services.AddScoped<FinTrack.Infrastructure.Services.DatabaseService>();

        // Register Services
        builder.Services.AddSingleton<ITransactionService, TransactionService>();
        builder.Services.AddSingleton<IBudgetService, BudgetService>();
        builder.Services.AddSingleton<IGoalService, GoalService>();
        
        // Register Sync and Connectivity Services
        builder.Services.AddSingleton<FinTrack.Core.Interfaces.IFeatureFlagService, FinTrack.Maui.Services.FeatureFlagService>();
        builder.Services.AddSingleton<FinTrack.Core.Interfaces.IConnectivityService, FinTrack.Maui.Services.ConnectivityService>();
        builder.Services.AddSingleton<FinTrack.Core.Interfaces.ISyncService, FinTrack.Shared.Services.SyncService>();

        // Register ViewModels
        builder.Services.AddTransient<ViewModels.TransactionsViewModel>();
        builder.Services.AddTransient<ViewModels.TransactionFormViewModel>();
        builder.Services.AddTransient<ViewModels.BudgetsViewModel>();
        builder.Services.AddTransient<ViewModels.BudgetFormViewModel>();
        builder.Services.AddTransient<ViewModels.GoalsViewModel>();
        builder.Services.AddTransient<ViewModels.GoalFormViewModel>();
        builder.Services.AddSingleton<ViewModels.SyncStatusViewModel>();

        // Register Pages
        builder.Services.AddTransient<Views.DashboardPage>();
        builder.Services.AddTransient<Views.TransactionsPage>();
        builder.Services.AddTransient<Views.TransactionFormPage>();
        builder.Services.AddTransient<Views.AccountsPage>();
        builder.Services.AddTransient<Views.ReportsPage>();
        builder.Services.AddTransient<Views.BudgetsPage>();
        builder.Services.AddTransient<Views.BudgetFormPage>();
        builder.Services.AddTransient<Views.GoalsPage>();
        builder.Services.AddTransient<Views.GoalFormPage>();
        builder.Services.AddTransient<Views.SyncStatusPage>();
        builder.Services.AddTransient<Views.FeatureFlagsPage>();
        
        // Register AppShell
        builder.Services.AddSingleton<AppShell>();





        return builder.Build();
    }
}
