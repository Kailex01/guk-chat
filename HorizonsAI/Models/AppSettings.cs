namespace HorizonsAI.Models;

public class AppSettings
{
    public string       OpenRouterApiKey     { get; set; } = "";
    public string       DefaultModel         { get; set; } = "openai/gpt-4o-mini";
    public string       SpeakerName          { get; set; } = "Player";
    public string       AuthorsNote          { get; set; } = "";
    public VoiceProfile NarratorVoiceProfile { get; set; } = new();
}
