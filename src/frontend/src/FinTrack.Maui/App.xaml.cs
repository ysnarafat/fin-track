using FinTrack.Infrastructure.Data;
using FinTrack.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Maui;

public partial class App : Application
{
	public App(AppShell appShell)
	{
		InitializeComponent();

		// Use AppShell for navigation with DI
		MainPage = appShell;
	}

	protected override async void OnStart()
	{
		base.OnStart();

		// Initialize database on app start
		await InitializeDatabaseAsync();
	}

	private async Task InitializeDatabaseAsync()
	{
		try
		{
			// Get the service provider from the current application
			var serviceProvider = IPlatformApplication.Current?.Services;
			if (serviceProvider != null)
			{
				using var scope = serviceProvider.CreateScope();
				var databaseService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
				await databaseService.InitializeDatabaseAsync();
			}
		}
		catch (Exception ex)
		{
			// Log error but don't crash the app
			System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
		}
	}
}
