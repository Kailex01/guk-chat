using System.Windows.Media;
using GukVoice.Models;

namespace GukVoice.ViewModels;

public class ActivityItem
{
    public LogEventType Type    { get; init; }
    public string       Tag     { get; init; } = "";
    public string       Text    { get; init; } = "";
    public string       Time    { get; init; } = "";

    // Colors resolved here so XAML doesn't need converters
    public Brush TagBackground => Type switch
    {
        LogEventType.Zone       => new SolidColorBrush(Color.FromRgb(0x10, 0x3A, 0x3A)),
        LogEventType.Experience => new SolidColorBrush(Color.FromRgb(0x3A, 0x30, 0x08)),
        LogEventType.Loot       => new SolidColorBrush(Color.FromRgb(0x10, 0x30, 0x10)),
        _                       => new SolidColorBrush(Color.FromRgb(0x18, 0x28, 0x48)),
    };

    public Brush TagForeground => Type switch
    {
        LogEventType.Zone       => new SolidColorBrush(Color.FromRgb(0x4E, 0xC9, 0xB0)),
        LogEventType.Experience => new SolidColorBrush(Color.FromRgb(0xDC, 0xDC, 0xAA)),
        LogEventType.Loot       => new SolidColorBrush(Color.FromRgb(0xB5, 0xCE, 0xA8)),
        _                       => new SolidColorBrush(Color.FromRgb(0x9C, 0xDC, 0xFE)),
    };
}
