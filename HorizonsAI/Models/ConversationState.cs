namespace HorizonsAI.Models;

public class ConversationState
{
    [JsonPropertyName("memory")]    public string?             Memory    { get; set; }
    [JsonPropertyName("sceneNpcs")] public List<SceneNpc>      SceneNpcs { get; set; } = new();
    [JsonPropertyName("messages")]  public List<ChatMessageDto> Messages  { get; set; } = new();
}
