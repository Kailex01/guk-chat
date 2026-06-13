using SharpCompress.Archives.GZip;
using SharpCompress.Common;

namespace GukVoice.Services;

public static class LogArchiveService
{
    public static void Archive(string logPath, string archiveFolder)
    {
        if (!File.Exists(logPath)) return;
        Directory.CreateDirectory(archiveFolder);

        var stamp       = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
        var archivePath = Path.Combine(archiveFolder, $"eqlog_{stamp}.log.gz");

        try
        {
            using var archive = GZipArchive.Create();
            archive.AddEntry(Path.GetFileName(logPath), logPath);
            archive.SaveTo(archivePath, CompressionType.GZip);

            // Truncate the original — EQ may still be open, so don't delete it
            File.WriteAllText(logPath, "");
        }
        catch { }
    }
}
