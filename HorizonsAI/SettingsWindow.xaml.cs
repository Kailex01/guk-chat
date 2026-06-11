using System.Windows.Input;
using HorizonsAI.Models;
using Microsoft.Win32;

namespace HorizonsAI;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        var s = AppConfig.Current;
        ApiKeyBox.Text        = s.OpenRouterApiKey;
        DefaultModelBox.Text  = s.DefaultModel;
        SpeakerNameBox.Text   = s.SpeakerName;
        PiperExeBox.Text      = s.PiperExePath;
        PiperModelsBox.Text   = s.PiperModelsPath;
        NarratorModelBox.Text = s.NarratorVoiceModel;
    }

    private void TitleBar_Drag(object sender, MouseButtonEventArgs e) => DragMove();

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        AppConfig.Apply(new AppSettings
        {
            OpenRouterApiKey   = ApiKeyBox.Text.Trim(),
            DefaultModel       = DefaultModelBox.Text.Trim(),
            SpeakerName        = SpeakerNameBox.Text.Trim(),
            PiperExePath       = PiperExeBox.Text.Trim(),
            PiperModelsPath    = PiperModelsBox.Text.Trim(),
            NarratorVoiceModel = NarratorModelBox.Text.Trim(),
        });
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }

    private void BrowsePiperExe_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Title = "Select piper.exe", Filter = "piper.exe|piper.exe|Executable|*.exe" };
        if (dlg.ShowDialog() == true)
            PiperExeBox.Text = dlg.FileName;
    }

    private void BrowsePiperModels_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog { Title = "Select Piper models folder" };
        if (dlg.ShowDialog() == true)
            PiperModelsBox.Text = dlg.FolderName;
    }
}
