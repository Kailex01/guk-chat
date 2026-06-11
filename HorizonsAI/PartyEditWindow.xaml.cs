using HorizonsAI.Models;
using HorizonsAI.Services;

namespace HorizonsAI;

public partial class PartyEditWindow : Window
{
    private Party? _editParty;
    private List<MemberCheckItem> _items = new();

    public PartyEditWindow()        => Init(null);
    public PartyEditWindow(Party p) => Init(p);

    private void Init(Party? party)
    {
        InitializeComponent();
        _editParty = party;

        if (party != null)
        {
            TitleText.Text       = "EDIT PARTY";
            NameBox.Text         = party.Name;
            ContextBox.Text      = party.Context;
            DeleteBtn.Visibility = Visibility.Visible;
        }

        var existing = party?.MemberIds ?? new List<string>();
        _items       = CharacterService.LoadAll()
                           .Select(c => new MemberCheckItem(c, existing.Contains(c.Id)))
                           .ToList();
        MemberList.ItemsSource = _items;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Party name is required.", "Horizon's AI",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var party = _editParty ?? new Party { Id = Party.MakeId(name) };
        party.Name      = name;
        party.Context   = ContextBox.Text.Trim();
        party.MemberIds = _items.Where(i => i.IsChecked).Select(i => i.Character.Id).ToList();

        PartyService.Save(party);
        DialogResult = true;
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_editParty == null) return;
        var result = MessageBox.Show($"Delete party \"{_editParty.Name}\"?", "Horizon's AI",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        PartyService.Delete(_editParty);
        DialogResult = false;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    private void TitleBar_Drag(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else DragMove();
    }

    // ── Inner helper ──────────────────────────────────────────────────────────

    private class MemberCheckItem : INotifyPropertyChanged
    {
        public Character    Character { get; }
        public string       Name      => Character.Name;
        public string       Category  => Character.Category.Replace('_', ' ');
        public BitmapImage? Portrait  { get; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(); }
        }

        public MemberCheckItem(Character c, bool isChecked)
        {
            Character  = c;
            _isChecked = isChecked;
            Portrait   = c.Portrait != null ? PortraitService.Load(c.Portrait) : null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
