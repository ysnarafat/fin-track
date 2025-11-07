namespace FinTrack.Maui;

public partial class App : Application
{
	public App(AppShell appShell)
	{
		InitializeComponent();

		// Use AppShell for navigation with DI
		MainPage = appShell;
	}
}
