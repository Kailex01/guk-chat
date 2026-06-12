namespace HorizonsAI.Models;

public class SceneNpc
{
    [JsonPropertyName("name")]        public string Name        { get; set; } = "";
    [JsonPropertyName("personality")] public string Personality { get; set; } = "";
}
