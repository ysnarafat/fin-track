namespace FinTrack.Maui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Use AppShell for navigation
		MainPage = new AppShell();
	}
}
