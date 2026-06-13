namespace GukVoice;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppConfig.Load();

        if (string.IsNullOrWhiteSpace(AppConfig.Current.EqLogPath) ||
            !File.Exists(AppConfig.Current.EqLogPath))
        {
            var setup = new SetupWindow();
            if (setup.ShowDialog() != true)
            {
                Shutdown();
                return;
            }
        }

        new MainWindow().Show();
    }
}
