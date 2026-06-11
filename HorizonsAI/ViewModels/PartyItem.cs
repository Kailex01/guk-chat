using HorizonsAI.Models;

namespace HorizonsAI.ViewModels;

public class PartyItem : INotifyPropertyChanged
{
    public Party Party { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public string DisplayName    => Party.Name;
    public string MemberCountText => Members.Count == 1 ? "1 member" : $"{Members.Count} members";
    public List<CharacterItem> Members { get; private set; } = new();

    public PartyItem(Party p) => Party = p;

    public void ResolveMembers(IEnumerable<CharacterItem> allCharacters)
    {
        Members = allCharacters
            .Where(c => Party.MemberIds.Contains(c.Character.Id))
            .ToList();
        OnPropertyChanged(nameof(Members));
        OnPropertyChanged(nameof(MemberCountText));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
