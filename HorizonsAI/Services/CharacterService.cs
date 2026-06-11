using HorizonsAI.Models;

namespace HorizonsAI.Services;

public static class CharacterService
{
    public static List<Character> LoadAll()
    {
        var chars = new List<Character>();
        if (!Directory.Exists(AppConfig.CharactersFolder)) return chars;

        foreach (var file in Directory.EnumerateFiles(AppConfig.CharactersFolder, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                var c = JsonSerializer.Deserialize<Character>(File.ReadAllText(file));
                if (c != null && c.Enabled) chars.Add(c);
            }
            catch { }
        }
        return chars.OrderBy(c => c.Category).ThenBy(c => c.Name).ToList();
    }

    public static void Save(Character c)
    {
        var folder = Path.Combine(AppConfig.CharactersFolder, c.Category);
        Directory.CreateDirectory(folder);
        File.WriteAllText(
            Path.Combine(folder, $"{c.Id}.json"),
            JsonSerializer.Serialize(c, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void Delete(Character c)
    {
        var path = Path.Combine(AppConfig.CharactersFolder, c.Category, $"{c.Id}.json");
        if (File.Exists(path)) File.Delete(path);

        if (!string.IsNullOrEmpty(c.Portrait))
        {
            var portraitPath = Path.Combine(AppConfig.PortraitsFolder, c.Portrait);
            if (File.Exists(portraitPath)) File.Delete(portraitPath);
        }
    }
}
