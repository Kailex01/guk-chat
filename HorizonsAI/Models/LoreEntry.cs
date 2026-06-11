namespace HorizonsAI.Models;

public class LoreEntry
{
    [JsonPropertyName("id")]       public string       Id       { get; set; } = "";
    [JsonPropertyName("title")]    public string       Title    { get; set; } = "";
    [JsonPropertyName("keywords")] public List<string> Keywords { get; set; } = new();
    [JsonPropertyName("content")]  public string       Content  { get; set; } = "";
    [JsonPropertyName("enabled")]  public bool         Enabled  { get; set; } = true;

    public string KeywordsDisplay => string.Join(", ", Keywords);

    public static string NewId() => Guid.NewGuid().ToString("N")[..12];
}
