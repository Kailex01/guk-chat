using HorizonsAI.Models;
using HorizonsAI.Services;

namespace HorizonsAI.ViewModels;

public class CharacterItem : INotifyPropertyChanged
{
    public Character Character { get; }

    private BitmapImage? _portrait;
    public BitmapImage? Portrait
    {
        get => _portrait;
        set { _portrait = value; OnPropertyChanged(); }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public string DisplayName   => Character.DisplayName;
    public string CategoryBadge => Character.CategoryBadge;
    public string Category      => Character.Category;

    public CharacterItem(Character c)
    {
        Character = c;
        Portrait  = PortraitService.Load(c.Portrait);
    }

    public void RefreshPortrait() => Portrait = PortraitService.Load(Character.Portrait);

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
