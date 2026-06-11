using HorizonsAI.Models;
using HorizonsAI.Services;

namespace HorizonsAI;

public static class AppConfig
{
    public static readonly string DataFolder       = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HorizonsAI");
    public static readonly string CharactersFolder = Path.Combine(DataFolder, "characters");
    public static readonly string PortraitsFolder  = Path.Combine(DataFolder, "portraits");
    public static readonly string ChatLogsFolder   = Path.Combine(DataFolder, "chatlogs");
    public static readonly string SettingsFile     = Path.Combine(DataFolder, "settings.json");
    public static readonly string LoreboookFile    = Path.Combine(DataFolder, "lorebook.json");

    public static AppSettings Current { get; private set; } = new();

    public static void Load()
    {
        EnsureFolders();
        Current = SettingsService.Load();
    }

    public static void Save()  => SettingsService.Save(Current);
    public static void Apply(AppSettings updated) { Current = updated; Save(); }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(CharactersFolder);
        Directory.CreateDirectory(PortraitsFolder);
        Directory.CreateDirectory(ChatLogsFolder);
    }
}
