using GukVoice.Models;

namespace GukVoice;

public static class AppConfig
{
    public static readonly string DataFolder    = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    public static readonly string SettingsFile  = Path.Combine(DataFolder, "settings.json");
    public static readonly string TtsFolder     = Path.Combine(DataFolder, "tts");
    public static readonly string ArchiveFolder = Path.Combine(DataFolder, "archive");

    public static GukVoiceSettings Current { get; private set; } = new();

    public static void Load()
    {
        Directory.CreateDirectory(DataFolder);
        Directory.CreateDirectory(TtsFolder);
        Directory.CreateDirectory(ArchiveFolder);

        if (!File.Exists(SettingsFile)) return;
        try
        {
            var json = File.ReadAllText(SettingsFile);
            Current = JsonSerializer.Deserialize<GukVoiceSettings>(json) ?? new();
        }
        catch { Current = new(); }
    }

    public static void Save()
    {
        var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsFile, json);
    }

    public static void Apply(GukVoiceSettings updated)
    {
        Current = updated;
        Save();
    }
}
