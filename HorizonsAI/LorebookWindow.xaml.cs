using System.Collections.ObjectModel;
using HorizonsAI.Models;
using HorizonsAI.Services;

namespace HorizonsAI;

public partial class LorebookWindow : Window
{
    private Lorebook _lorebook = new();
    private ObservableCollection<LoreEntry> _entries = new();

    public LorebookWindow()
    {
        InitializeComponent();
        _lorebook        = LorebookService.Load();
        _entries         = new ObservableCollection<LoreEntry>(_lorebook.Entries);
        EntryList.ItemsSource = _entries;
        UpdateCount();
        ShowEditor(false);
    }

    // ── Entry list ─────────────────────────────────────────────────────────────

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var entry = new LoreEntry { Id = LoreEntry.NewId(), Title = "New Entry", Enabled = true };
        _entries.Add(entry);
        EntryList.SelectedItem = entry;
        UpdateCount();
        TitleBox.Focus();
        TitleBox.SelectAll();
    }

    private void EntryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var entry = EntryList.SelectedItem as LoreEntry;
        if (entry == null) { ShowEditor(false); return; }
        ShowEditor(true);
        TitleBox.Text          = entry.Title;
        KeywordsBox.Text       = string.Join(", ", entry.Keywords);
        ContentBox.Text        = entry.Content;
        EnabledCheck.IsChecked = entry.Enabled;
    }

    // ── Editor actions ─────────────────────────────────────────────────────────

    private void SaveEntry_Click(object sender, RoutedEventArgs e)
    {
        var entry = EntryList.SelectedItem as LoreEntry;
        if (entry == null) return;

        entry.Title    = TitleBox.Text.Trim();
        entry.Keywords = KeywordsBox.Text
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        entry.Content  = ContentBox.Text.Trim();
        entry.Enabled  = EnabledCheck.IsChecked == true;

        // Force ListBox to refresh the item display
        var idx = _entries.IndexOf(entry);
        _entries.RemoveAt(idx);
        _entries.Insert(idx, entry);
        EntryList.SelectedIndex = idx;

        Save();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        var entry = EntryList.SelectedItem as LoreEntry;
        if (entry == null) return;
        var result = MessageBox.Show($"Delete \"{entry.Title}\"?", "Lorebook",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        _entries.Remove(entry);
        ShowEditor(false);
        UpdateCount();
        Save();
    }

    // ── Window ─────────────────────────────────────────────────────────────────

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void TitleBar_Drag(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else DragMove();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        Save();
        base.OnClosing(e);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private void Save()
    {
        _lorebook.Entries = _entries.ToList();
        LorebookService.Save(_lorebook);
    }

    private void ShowEditor(bool show)
    {
        EditorPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        EmptyHint.Visibility   = show ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateCount()
    {
        EntryCountText.Text = _entries.Count == 1 ? "1 entry" : $"{_entries.Count} entries";
    }
}
