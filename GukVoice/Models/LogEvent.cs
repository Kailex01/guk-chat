namespace GukVoice.Models;

public enum LogEventType { Chat, Zone, Experience, Loot }

public class LogEvent
{
    public LogEventType Type    { get; init; }
    public DateTime     Time    { get; init; }
    public string       Speaker { get; init; } = "";
    public string       Text    { get; init; } = "";
}
