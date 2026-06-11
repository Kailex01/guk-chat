using System.IO;
using System.Net.Http;
using System.Windows.Input;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace HorizonsAI;

public partial class TtsSetupWindow : Window
{
    private bool _useMultilingual = false; // false = English-only (default)
    private CancellationTokenSource? _cts;

    public bool Downloaded { get; private set; } = false;

    private const string EnUrl    = "https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models/kokoro-en-v0_19.tar.bz2";
    private const string MultiUrl = "https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models/kokoro-multi-lang-v1_1.tar.bz2";

    public TtsSetupWindow() => InitializeComponent();

    private void TitleBar_Drag(object sender, MouseButtonEventArgs e) => DragMove();

    // ── Selection ──────────────────────────────────────────────────────────────

    private void SelectFull_Click(object sender, MouseButtonEventArgs e)
    {
        _useMultilingual = true;
        SetBorderSelected(FullBorder, FullDot, true);
        SetBorderSelected(CompactBorder, CompactDot, false);
    }

    private void SelectCompact_Click(object sender, MouseButtonEventArgs e)
    {
        _useMultilingual = false;
        SetBorderSelected(CompactBorder, CompactDot, true);
        SetBorderSelected(FullBorder, FullDot, false);
    }

    private static void SetBorderSelected(System.Windows.Controls.Border border,
                                          System.Windows.Shapes.Ellipse  dot,
                                          bool selected)
    {
        border.BorderBrush = selected
            ? new SolidColorBrush(Color.FromRgb(0xC8, 0xA0, 0x20))
            : new SolidColorBrush(Color.FromRgb(0x1E, 0x3A, 0x50));
        border.Background = selected
            ? new SolidColorBrush(Color.FromRgb(0x11, 0x1E, 0x2A))
            : new SolidColorBrush(Color.FromRgb(0x0A, 0x10, 0x18));
        dot.Fill = selected
            ? new SolidColorBrush(Color.FromRgb(0xC8, 0xA0, 0x20))
            : new SolidColorBrush(Colors.Transparent);
    }

    // ── Download ───────────────────────────────────────────────────────────────

    private async void Download_Click(object sender, RoutedEventArgs e)
    {
        DownloadBtn.IsEnabled = false;
        SkipBtn.IsEnabled     = false;
        ProgressPanel.Visibility = Visibility.Visible;

        _cts = new CancellationTokenSource();
        var url       = _useMultilingual ? MultiUrl : EnUrl;
        var modelType = _useMultilingual ? "multi-v1_1" : "en-v0_19";

        try
        {
            await DownloadAndExtractAsync(url, AppConfig.TtsFolder, modelType, _cts.Token);
            Downloaded   = true;
            DialogResult = true;
            Close();
        }
        catch (OperationCanceledException)
        {
            ProgressText.Text     = "Cancelled.";
            DownloadBtn.IsEnabled = true;
            SkipBtn.IsEnabled     = true;
        }
        catch (Exception ex)
        {
            ProgressText.Text     = $"Error: {ex.Message}";
            DownloadBtn.IsEnabled = true;
            SkipBtn.IsEnabled     = true;
        }
    }

    private async Task DownloadAndExtractAsync(string url, string destFolder,
                                                string modelType, CancellationToken ct)
    {
        Directory.CreateDirectory(destFolder);

        using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(60) };

        using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var total   = response.Content.Headers.ContentLength ?? -1L;
        var tmpPath = Path.Combine(Path.GetTempPath(), "horizons_kokoro_download.tar.bz2");

        // ── Download ───────────────────────────────────────────────────────────
        await using (var src = await response.Content.ReadAsStreamAsync(ct))
        await using (var dst = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, true))
        {
            var buf        = new byte[65536];
            long downloaded = 0;
            int  read;

            while ((read = await src.ReadAsync(buf, ct)) > 0)
            {
                await dst.WriteAsync(buf.AsMemory(0, read), ct);
                downloaded += read;

                if (total > 0)
                {
                    var pct    = (int)(downloaded * 100L / total);
                    var mb     = downloaded / 1_048_576.0;
                    var totalM = total      / 1_048_576.0;
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar.Value  = pct;
                        ProgressText.Text  = $"Downloading… {mb:F0} / {totalM:F0} MB ({pct}%)";
                    });
                }
            }
        }

        // ── Extract ────────────────────────────────────────────────────────────
        Dispatcher.Invoke(() =>
        {
            ProgressBar.Value = 100;
            ProgressText.Text = "Extracting archive…";
        });

        await Task.Run(() => ExtractTarBz2(tmpPath, destFolder), ct);

        // Write model-type marker so KokoroService picks the right SID table
        await File.WriteAllTextAsync(Path.Combine(destFolder, "model_type.txt"), modelType, ct);

        try { File.Delete(tmpPath); } catch { /* best-effort cleanup */ }
    }

    private static void ExtractTarBz2(string archivePath, string destFolder)
    {
        using var archive = ArchiveFactory.Open(archivePath);

        foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
        {
            // Strip the top-level folder name (e.g. "kokoro-en-v0_19/model.onnx" → "model.onnx")
            var parts = entry.Key?.Replace('\\', '/').Split('/') ?? [];
            var rel   = string.Join(Path.DirectorySeparatorChar.ToString(),
                                    parts.Length > 1 ? parts.Skip(1) : parts);
            if (string.IsNullOrEmpty(rel)) continue;

            var dest = Path.Combine(destFolder, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);

            using var src  = entry.OpenEntryStream();
            using var file = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None, 65536);
            src.CopyTo(file);
        }
    }

    // ── Skip ───────────────────────────────────────────────────────────────────

    private void Skip_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        DialogResult = false;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _cts?.Cancel();
        base.OnClosed(e);
    }
}
