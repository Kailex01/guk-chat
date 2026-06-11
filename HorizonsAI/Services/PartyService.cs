using HorizonsAI.Models;

namespace HorizonsAI.Services;

public static class PartyService
{
    private static string PartiesFolder => Path.Combine(AppConfig.DataFolder, "parties");

    public static List<Party> LoadAll()
    {
        var list = new List<Party>();
        if (!Directory.Exists(PartiesFolder)) return list;
        foreach (var file in Directory.EnumerateFiles(PartiesFolder, "*.json"))
        {
            try
            {
                var p = JsonSerializer.Deserialize<Party>(File.ReadAllText(file));
                if (p != null) list.Add(p);
            }
            catch { }
        }
        return list.OrderBy(p => p.Name).ToList();
    }

    public static void Save(Party p)
    {
        Directory.CreateDirectory(PartiesFolder);
        File.WriteAllText(
            Path.Combine(PartiesFolder, $"{p.Id}.json"),
            JsonSerializer.Serialize(p, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void Delete(Party p)
    {
        var path = Path.Combine(PartiesFolder, $"{p.Id}.json");
        if (File.Exists(path)) File.Delete(path);
    }
}
