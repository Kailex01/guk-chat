namespace GukVoice.Models;

public enum SpeakerType { Npc, Player, Bot }

public class SpeakerProfile
{
    [JsonPropertyName("name")]          public string       Name         { get; set; } = "";
    [JsonPropertyName("type")]          public SpeakerType  Type         { get; set; } = SpeakerType.Npc;
    [JsonPropertyName("voice_profile")] public VoiceProfile VoiceProfile { get; set; } = new();
    [JsonPropertyName("enabled")]       public bool         Enabled      { get; set; } = true;
}
