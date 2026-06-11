namespace HorizonsAI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Register first so any startup exception is caught and shown rather than silently killing the app
        DispatcherUnhandledException += (_, ex) =>
        {
            MessageBox.Show(
                $"Unexpected error:\n\n{ex.Exception.Message}",
                "Horizon's AI", MessageBoxButton.OK, MessageBoxImage.Error);
            ex.Handled = true;
        };

        try { AppConfig.Load(); }
        catch
        {
            // Corrupt or missing settings file — start fresh
            AppConfig.Reset();
        }

        base.OnStartup(e);
    }
}
