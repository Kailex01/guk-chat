namespace HorizonsAI.Services;

public static class PortraitService
{
    private const int TargetSize = 512;

    public static string? Import(string sourcePath, string characterId)
    {
        try
        {
            var destFile = $"{characterId}.png";
            var destPath = Path.Combine(AppConfig.PortraitsFolder, destFile);
            Directory.CreateDirectory(AppConfig.PortraitsFolder);

            using var src = System.Drawing.Image.FromFile(sourcePath);
            using var bmp = CropSquareAndResize(src);
            bmp.Save(destPath, System.Drawing.Imaging.ImageFormat.Png);
            return destFile;
        }
        catch { return null; }
    }

    public static BitmapImage? Load(string? filename)
    {
        if (string.IsNullOrEmpty(filename)) return null;
        var path = Path.Combine(AppConfig.PortraitsFolder, filename);
        if (!File.Exists(path)) return null;
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource   = new Uri(path);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }

    private static System.Drawing.Bitmap CropSquareAndResize(System.Drawing.Image src)
    {
        int size = Math.Min(src.Width, src.Height);
        int x    = (src.Width  - size) / 2;
        int y    = (src.Height - size) / 2;

        var dest = new System.Drawing.Bitmap(TargetSize, TargetSize);
        using var g = System.Drawing.Graphics.FromImage(dest);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.DrawImage(src,
            new System.Drawing.Rectangle(0, 0, TargetSize, TargetSize),
            new System.Drawing.Rectangle(x, y, size, size),
            System.Drawing.GraphicsUnit.Pixel);
        return dest;
    }
}
