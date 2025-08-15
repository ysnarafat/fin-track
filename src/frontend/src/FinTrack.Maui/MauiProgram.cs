using FinTrack.Maui.Data;
using FinTrack.Maui.Services;
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

        // Register Services
        builder.Services.AddSingleton<ITransactionService, TransactionService>();
        builder.Services.AddSingleton<IBudgetService, BudgetService>();
        builder.Services.AddSingleton<IGoalService, GoalService>();
        builder.Services.AddSingleton<WeatherForecastService>();

        // Register ViewModels
        builder.Services.AddTransient<ViewModels.TransactionsViewModel>();
        builder.Services.AddTransient<ViewModels.TransactionFormViewModel>();
        builder.Services.AddTransient<ViewModels.BudgetsViewModel>();
        builder.Services.AddTransient<ViewModels.BudgetFormViewModel>();
        builder.Services.AddTransient<ViewModels.GoalsViewModel>();
        builder.Services.AddTransient<ViewModels.GoalFormViewModel>();

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

        return builder.Build();
    }
}
