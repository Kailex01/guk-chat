namespace HorizonsAI.Models;

public class Party
{
    [JsonPropertyName("id")]       public string       Id        { get; set; } = "";
    [JsonPropertyName("name")]     public string       Name      { get; set; } = "";
    [JsonPropertyName("context")]  public string       Context   { get; set; } = "";
    [JsonPropertyName("members")]  public List<string> MemberIds { get; set; } = new();

    public static string MakeId(string name) => Character.MakeId(name);
}
