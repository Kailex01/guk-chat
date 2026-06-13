using GukVoice.Models;
using Microsoft.Win32;

namespace GukVoice;

public partial class SetupWindow : Window
{
    public SetupWindow()
    {
        InitializeComponent();
        // Pre-fill from existing settings if reopened from settings menu
        LogPathBox.Text     = AppConfig.Current.EqLogPath;
        PlayerNameBox.Text  = AppConfig.Current.PlayerName;
        ArchiveFolderBox.Text = AppConfig.Current.ArchiveFolder;
    }

    private void BrowseLog_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Select EverQuest Log File",
            Filter = "Log files (*.txt)|*.txt|All files (*.*)|*.*",
        };
        if (dlg.ShowDialog() == true)
            LogPathBox.Text = dlg.FileName;
    }

    private void BrowseArchive_Click(object sender, RoutedEventArgs e)
    {
        // Use the folder browser trick via SaveFileDialog with a dummy file
        var dlg = new OpenFileDialog
        {
            Title            = "Select Archive Folder (pick any file inside it)",
            Filter           = "All files (*.*)|*.*",
            CheckFileExists  = false,
            FileName         = "Select Folder",
        };
        if (dlg.ShowDialog() == true)
            ArchiveFolderBox.Text = Path.GetDirectoryName(dlg.FileName) ?? "";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LogPathBox.Text))
        {
            MessageBox.Show("Please select the EQ log file.", "GukVoice",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        AppConfig.Apply(new GukVoiceSettings
        {
            EqLogPath     = LogPathBox.Text.Trim(),
            PlayerName    = PlayerNameBox.Text.Trim(),
            ArchiveFolder = ArchiveFolderBox.Text.Trim(),
            ArchiveOnEqExit = AppConfig.Current.ArchiveOnEqExit,
            Speakers      = AppConfig.Current.Speakers,
            ZoneVoice     = AppConfig.Current.ZoneVoice,
            ExpVoice      = AppConfig.Current.ExpVoice,
            LootVoice     = AppConfig.Current.LootVoice,
        });

        DialogResult = true;
    }
}
