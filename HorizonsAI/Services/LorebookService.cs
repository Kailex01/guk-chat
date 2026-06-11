using HorizonsAI.Models;

namespace HorizonsAI.Services;

public static class LorebookService
{
    private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    public static Lorebook Load()
    {
        if (!File.Exists(AppConfig.LoreboookFile)) return new Lorebook();
        try { return JsonSerializer.Deserialize<Lorebook>(File.ReadAllText(AppConfig.LoreboookFile)) ?? new Lorebook(); }
        catch { return new Lorebook(); }
    }

    public static void Save(Lorebook lorebook)
    {
        Directory.CreateDirectory(AppConfig.DataFolder);
        File.WriteAllText(AppConfig.LoreboookFile, JsonSerializer.Serialize(lorebook, _opts));
    }
}
