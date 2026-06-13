using GukVoice.Kokoro.Services;
using HorizonsAI.Models;
using HorizonsAI.Services;
using HorizonsAI.ViewModels;

namespace HorizonsAI;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;

        _vm.ScrollToBottom += () =>
            Dispatcher.InvokeAsync(
                () => MessagesScroll.ScrollToEnd(),
                System.Windows.Threading.DispatcherPriority.Background);

        _vm.TtsSetupRequested += OnTtsSetupRequested;

        Loaded += (_, _) =>
        {
            _vm.LoadCharacters(); // LoadCharacters calls LoadParties internally
            _vm.LoadScenes();
            if (KokoroService.IsModelReady(AppConfig.TtsFolder))
            {
                try { _vm.InitializeTts(); }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Voice model found but failed to load:\n\n{ex.Message}\n\nDelete the data/tts folder and re-download the model.",
                        "Voice Setup", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        };

        Closed += (_, _) => _vm.Dispose();
    }

    // ── TTS setup ─────────────────────────────────────────────────────────────

    private bool _ttsSetupInProgress;
    private void OnTtsSetupRequested()
    {
        if (_ttsSetupInProgress) return;
        _ttsSetupInProgress = true;
        // BeginInvoke defers so we're not running dialog code inside the property setter
        Dispatcher.BeginInvoke(() =>
        {
            try
            {
                var dlg = new TtsSetupWindow { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    try { _vm.InitializeTts(); }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Voice model loaded but failed to initialise:\n\n{ex.Message}\n\nTry restarting the app.",
                            "Voice Setup", MessageBoxButton.OK, MessageBoxImage.Warning);
                        _vm.IsVoiceEnabled = false;
                    }
                }
                else
                    _vm.IsVoiceEnabled = false;
            }
            finally
            {
                _ttsSetupInProgress = false;
            }
        });
    }

    // ── Title bar ──────────────────────────────────────────────────────────────

    private void TitleBar_Drag(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) ToggleMaximize();
        else DragMove();
    }
    private void Minimize_Click(object sender, RoutedEventArgs e)   => WindowState = WindowState.Minimized;
    private void MaxRestore_Click(object sender, RoutedEventArgs e) => ToggleMaximize();
    private void Close_Click(object sender, RoutedEventArgs e)      => Close();
    private void ToggleMaximize() =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    // ── Character sidebar ──────────────────────────────────────────────────────

    private void CategoryHeader_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).Tag is CategoryGroup group)
            group.IsExpanded = !group.IsExpanded;
    }

    private void AddCharacter_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new CharacterEditWindow { Owner = this };
        if (dlg.ShowDialog() == true) _vm.LoadCharacters();
    }

    private void EditCharacter_Click(object sender, RoutedEventArgs e)
    {
        var item = GetContextItem<CharacterItem>(sender);
        if (item is null) return;
        var dlg = new CharacterEditWindow(item.Character) { Owner = this };
        if (dlg.ShowDialog() != null) _vm.LoadCharacters();
    }

    private void DeleteCharacter_Click(object sender, RoutedEventArgs e)
    {
        var item = GetContextItem<CharacterItem>(sender);
        if (item != null) _vm.DeleteCharacter(item);
    }

    // ── Party sidebar ──────────────────────────────────────────────────────────

    private void AddParty_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new PartyEditWindow { Owner = this };
        if (dlg.ShowDialog() == true) _vm.LoadParties();
    }

    private void EditParty_Click(object sender, RoutedEventArgs e)
    {
        var item = GetContextItem<PartyItem>(sender);
        if (item is null) return;
        var dlg = new PartyEditWindow(item.Party) { Owner = this };
        if (dlg.ShowDialog() != null) _vm.LoadParties();
    }

    private void DeleteParty_Click(object sender, RoutedEventArgs e)
    {
        var item = GetContextItem<PartyItem>(sender);
        if (item != null) _vm.DeleteParty(item);
    }

    // ── Scene sidebar ──────────────────────────────────────────────────────────

    private void AddScene_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SceneEditWindow { Owner = this };
        if (dlg.ShowDialog() == true) _vm.LoadScenes();
    }

    private void EditScene_Click(object sender, RoutedEventArgs e)
    {
        var item = GetContextItem<SceneItem>(sender);
        if (item is null) return;
        var dlg = new SceneEditWindow(item.Scene) { Owner = this };
        if (dlg.ShowDialog() != null) _vm.LoadScenes();
    }

    private void DeleteScene_Click(object sender, RoutedEventArgs e)
    {
        var item = GetContextItem<SceneItem>(sender);
        if (item != null) _vm.DeleteScene(item);
    }

    private void PromoteSceneNpc_Click(object sender, RoutedEventArgs e)
    {
        var npc = GetContextItem<SceneNpc>(sender);
        if (npc != null) _vm.PromoteSceneNpc(npc);
    }

    // ── Play-as picker ────────────────────────────────────────────────────────

    private void PlayAsBtn_Click(object sender, RoutedEventArgs e)
        => PlayAsPopup.IsOpen = !PlayAsPopup.IsOpen;

    private void PlayAsPlayer_Click(object sender, RoutedEventArgs e)
    {
        _vm.SetPlayAs(null);
        PlayAsPopup.IsOpen = false;
    }

    private void PlayAsCharacter_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).Tag is CharacterItem item)
            _vm.SetPlayAs(item);
        PlayAsPopup.IsOpen = false;
    }

    // ── Dialogs ────────────────────────────────────────────────────────────────

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SettingsWindow { Owner = this };
        if (dlg.ShowDialog() == true) _vm.OnSettingsChanged();
    }

    private void Lorebook_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new LorebookWindow { Owner = this };
        dlg.ShowDialog();
        _vm.LoadLorebook();
    }

    // ── Message edit ──────────────────────────────────────────────────────────

    private void EditMessage_Click(object sender, RoutedEventArgs e)
    {
        var vm = GetContextItem<ChatMessageVm>(sender);
        vm?.BeginEdit();
    }

    // ── Input ──────────────────────────────────────────────────────────────────

    private int    _historyIndex = -1;
    private string _historyDraft = "";

    private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var history = _vm.InputHistory;
        if (e.Key == Key.Up && InputBox.GetLineIndexFromCharacterIndex(InputBox.CaretIndex) == 0)
        {
            if (history.Count == 0) { e.Handled = true; return; }
            if (_historyIndex == -1) _historyDraft = InputBox.Text;
            _historyIndex = Math.Min(_historyIndex + 1, history.Count - 1);
            InputBox.Text = history[_historyIndex];
            InputBox.CaretIndex = InputBox.Text.Length;
            e.Handled = true;
        }
        else if (e.Key == Key.Down && _historyIndex >= 0
                 && InputBox.GetLineIndexFromCharacterIndex(InputBox.CaretIndex) == InputBox.LineCount - 1)
        {
            _historyIndex--;
            InputBox.Text = _historyIndex >= 0 ? history[_historyIndex] : _historyDraft;
            if (_historyIndex < 0) _historyIndex = -1;
            InputBox.CaretIndex = InputBox.Text.Length;
            e.Handled = true;
        }
    }

    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        // Enter without Shift = send; Shift+Enter = newline (AcceptsReturn handles it)
        if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == 0
            && _vm.SendCommand.CanExecute(null))
        {
            _historyIndex = -1;
            _vm.SendCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void QuickInsert_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement el || el.Tag is not string token) return;
        int   caret  = InputBox.CaretIndex;
        var   prefix = caret > 0 && InputBox.Text.Length > 0 && InputBox.Text[caret - 1] != ' ' ? " " : "";
        InputBox.Text       = InputBox.Text.Insert(caret, prefix + token);
        InputBox.CaretIndex = caret + prefix.Length + token.Length;
        InputBox.Focus();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static T? GetContextItem<T>(object sender) where T : class
    {
        if (sender is MenuItem mi && mi.Parent is ContextMenu cm)
            return cm.DataContext as T;
        return null;
    }
}
