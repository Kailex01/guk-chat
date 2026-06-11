using System.Windows.Input;
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

        Loaded += (_, _) => _vm.LoadCharacters();
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

    // ── Sidebar ────────────────────────────────────────────────────────────────

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
        var item = GetContextItem(sender);
        if (item is null) return;
        var dlg = new CharacterEditWindow(item.Character) { Owner = this };
        if (dlg.ShowDialog() != null) _vm.LoadCharacters();
    }

    private void DeleteCharacter_Click(object sender, RoutedEventArgs e)
    {
        var item = GetContextItem(sender);
        if (item != null) _vm.DeleteCharacter(item);
    }

    private static CharacterItem? GetContextItem(object sender)
    {
        if (sender is MenuItem mi && mi.Parent is ContextMenu cm)
            return cm.DataContext as CharacterItem;
        return null;
    }

    // ── Dialogs ────────────────────────────────────────────────────────────────

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SettingsWindow { Owner = this };
        if (dlg.ShowDialog() == true) _vm.OnSettingsChanged();
    }

    private void Lorebook_Click(object sender, RoutedEventArgs e)
    {
        // Phase 2.6 — placeholder
        MessageBox.Show("Lorebook editor coming soon.", "Horizon's AI",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ── Input ──────────────────────────────────────────────────────────────────

    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _vm.SendCommand.CanExecute(null))
        {
            _vm.SendCommand.Execute(null);
            e.Handled = true;
        }
    }
}
