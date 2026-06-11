using HorizonsAI.Models;

namespace HorizonsAI.Services;

public static class SettingsService
{
    public static AppSettings Load()
    {
        var path = AppConfig.SettingsFile;
        try
        {
            if (File.Exists(path))
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new AppSettings();
        }
        catch { }
        return new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(AppConfig.SettingsFile)!);
        File.WriteAllText(AppConfig.SettingsFile,
            JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }
}
