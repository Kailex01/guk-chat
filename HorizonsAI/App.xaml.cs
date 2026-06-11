namespace HorizonsAI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppConfig.Load();
        base.OnStartup(e);

        // Catch unhandled exceptions on the UI thread and show them instead of silently dying
        DispatcherUnhandledException += (_, ex) =>
        {
            MessageBox.Show(
                $"Unexpected error:\n\n{ex.Exception.Message}",
                "Horizon's AI", MessageBoxButton.OK, MessageBoxImage.Error);
            ex.Handled = true;
        };
    }
}
