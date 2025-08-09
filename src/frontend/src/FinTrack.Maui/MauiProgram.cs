using Microsoft.Extensions.Logging;
using FinTrack.Maui.Data;
using FinTrack.Maui.Services;

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
		builder.Services.AddSingleton<WeatherForecastService>();

		// Register ViewModels
		builder.Services.AddTransient<FinTrack.Maui.ViewModels.TransactionsViewModel>();
		builder.Services.AddTransient<FinTrack.Maui.ViewModels.TransactionFormViewModel>();

		// Register Pages
		builder.Services.AddTransient<FinTrack.Maui.Views.DashboardPage>();
		builder.Services.AddTransient<FinTrack.Maui.Views.TransactionsPage>();
		builder.Services.AddTransient<FinTrack.Maui.Views.TransactionFormPage>();
		builder.Services.AddTransient<FinTrack.Maui.Views.AccountsPage>();
		builder.Services.AddTransient<FinTrack.Maui.Views.ReportsPage>();

		return builder.Build();
	}
}
