using GukVoice.Models;

namespace GukVoice.ViewModels;

public class SpeakerItem : INotifyPropertyChanged
{
    public SpeakerProfile Profile { get; }

    public string      Name => Profile.Name;
    public SpeakerType Type => Profile.Type;

    private bool _isSpeaking;
    public bool IsSpeaking
    {
        get => _isSpeaking;
        set { _isSpeaking = value; OnPropertyChanged(); }
    }

    private int _pendingCount;
    public int PendingCount
    {
        get => _pendingCount;
        set { _pendingCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPending)); }
    }

    public bool HasPending => PendingCount > 0;

    public SpeakerItem(SpeakerProfile profile) => Profile = profile;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
