using HorizonsAI.Models;
using HorizonsAI.Services;

namespace HorizonsAI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly HttpClient        _http   = new() { Timeout = TimeSpan.FromSeconds(60) };
    private readonly OpenRouterService _openRouter;
    private readonly Dictionary<string, ObservableCollection<ChatMessage>> _conversations = new();

    // ── Sidebar ────────────────────────────────────────────────────────────────

    public ObservableCollection<CategoryGroup> Categories { get; } = new();

    private CharacterItem? _selectedCharacter;
    public CharacterItem? SelectedCharacter
    {
        get => _selectedCharacter;
        set
        {
            if (_selectedCharacter == value) return;
            if (_selectedCharacter != null) _selectedCharacter.IsSelected = false;
            _selectedCharacter = value;
            if (_selectedCharacter != null) _selectedCharacter.IsSelected = true;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ActivePortrait));
            OnPropertyChanged(nameof(ActiveCharacterName));
            OnPropertyChanged(nameof(ActiveCategoryBadge));
            OnPropertyChanged(nameof(HasActiveCharacter));
            SwitchConversation();
        }
    }

    public ICommand SelectCharacterCommand { get; }

    // ── Active character header ────────────────────────────────────────────────

    public BitmapImage? ActivePortrait      => _selectedCharacter?.Portrait;
    public string       ActiveCharacterName => _selectedCharacter?.DisplayName ?? "Select a character";
    public string       ActiveCategoryBadge => _selectedCharacter?.CategoryBadge ?? "";
    public bool         HasActiveCharacter  => _selectedCharacter != null;

    // ── Messages ───────────────────────────────────────────────────────────────

    private ObservableCollection<ChatMessage> _messages = new();
    public ObservableCollection<ChatMessage> Messages
    {
        get => _messages;
        private set { _messages = value; OnPropertyChanged(); }
    }

    // ── Input / status ─────────────────────────────────────────────────────────

    private string _inputText = "";
    public string InputText
    {
        get => _inputText;
        set { _inputText = value; OnPropertyChanged(); }
    }

    private bool _isSending;
    public bool IsSending
    {
        get => _isSending;
        set
        {
            _isSending = value;
            OnPropertyChanged();
            Application.Current.Dispatcher.InvokeAsync(CommandManager.InvalidateRequerySuggested);
        }
    }

    private string _statusText = "";
    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasStatus)); }
    }

    public bool HasStatus => !string.IsNullOrEmpty(_statusText);

    private bool _isVoiceEnabled;
    public bool IsVoiceEnabled
    {
        get => _isVoiceEnabled;
        set { _isVoiceEnabled = value; OnPropertyChanged(); }
    }

    // ── Commands ───────────────────────────────────────────────────────────────

    public ICommand SendCommand { get; }

    // ── Scroll signal ──────────────────────────────────────────────────────────

    public event Action? ScrollToBottom;

    // ── Constructor ────────────────────────────────────────────────────────────

    public MainViewModel()
    {
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("HorizonsAI/1.0");
        _openRouter = new OpenRouterService(_http);

        SendCommand = new RelayCommand(
            async _ => await SendMessageAsync(),
            _ => !IsSending && SelectedCharacter != null && !string.IsNullOrWhiteSpace(InputText));

        SelectCharacterCommand = new RelayCommand(
            p => { if (p is CharacterItem item) SelectedCharacter = item; return Task.CompletedTask; });
    }

    // ── Character management ───────────────────────────────────────────────────

    public void LoadCharacters()
    {
        var previousId  = _selectedCharacter?.Character.Id;
        var allChars    = CharacterService.LoadAll();

        Categories.Clear();

        foreach (var group in allChars.GroupBy(c => c.Category))
        {
            var cat = new CategoryGroup(group.Key);
            foreach (var c in group)
                cat.Characters.Add(new CharacterItem(c));
            Categories.Add(cat);
        }

        // Restore selection if the character still exists
        if (previousId != null)
        {
            var match = Categories.SelectMany(g => g.Characters)
                                  .FirstOrDefault(i => i.Character.Id == previousId);
            if (match != null) SelectedCharacter = match;
        }

        StatusText = Categories.Count == 0
            ? "No characters yet — click [+] to add one."
            : "";
    }

    public void DeleteCharacter(CharacterItem item)
    {
        CharacterService.Delete(item.Character);
        if (SelectedCharacter == item) SelectedCharacter = null;
        LoadCharacters();
    }

    public void OnSettingsChanged() { }

    // ── Conversation management ────────────────────────────────────────────────

    private void SwitchConversation()
    {
        if (_selectedCharacter is null) return;
        var key = _selectedCharacter.Character.Id;
        if (!_conversations.ContainsKey(key))
            _conversations[key] = new ObservableCollection<ChatMessage>();
        Messages   = _conversations[key];
        StatusText = "";
        ScrollToBottom?.Invoke();
    }

    // ── Send message ───────────────────────────────────────────────────────────

    private async Task SendMessageAsync()
    {
        if (SelectedCharacter is null || string.IsNullOrWhiteSpace(InputText)) return;
        var text = InputText.Trim();
        InputText = "";
        IsSending = true;

        Messages.Add(new ChatMessage
        {
            Text       = text,
            IsPlayer   = true,
            SenderName = AppConfig.Current.SpeakerName,
            Timestamp  = DateTime.Now,
        });
        ScrollToBottom?.Invoke();

        try
        {
            StatusText = $"{SelectedCharacter.DisplayName} is thinking…";
            var lines      = await _openRouter.ChatAsync(SelectedCharacter.Character, Messages.SkipLast(0), text);
            var charName   = SelectedCharacter.DisplayName;
            var voiceModel = SelectedCharacter.Character.VoiceModel;

            foreach (var line in lines)
            {
                Messages.Add(new ChatMessage
                {
                    Text       = line,
                    IsPlayer   = false,
                    SenderName = charName,
                    Portrait   = SelectedCharacter.Portrait,
                    Timestamp  = DateTime.Now,
                });
            }
            ScrollToBottom?.Invoke();
            StatusText = "";

            if (IsVoiceEnabled)
                _ = PiperService.SpeakLinesAsync(lines, charName, voiceModel);
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsSending = false;
        }
    }

    // ── INotifyPropertyChanged ─────────────────────────────────────────────────

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
