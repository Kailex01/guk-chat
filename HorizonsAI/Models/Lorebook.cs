namespace HorizonsAI.Models;

public class Lorebook
{
    [JsonPropertyName("entries")] public List<LoreEntry> Entries { get; set; } = new();
}
