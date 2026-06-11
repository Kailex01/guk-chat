namespace HorizonsAI.ViewModels;

public class CategoryGroup : INotifyPropertyChanged
{
    public string Name { get; }
    public ObservableCollection<CharacterItem> Characters { get; } = new();

    private bool _isExpanded = true;
    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; OnPropertyChanged(); }
    }

    public string DisplayName => Name.Replace('_', ' ').ToUpper();

    public CategoryGroup(string name) => Name = name;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
