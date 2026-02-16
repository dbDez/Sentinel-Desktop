using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

// Use the better icon from the icons folder (playstore-icon is already square and high-quality)
var pngPath = @"C:\Sentinel\icons\android\playstore-icon.png";
var icoPath = @"C:\Sentinel\SafetySentinel\Assets\sentinel.ico";
var assetPngPath = @"C:\Sentinel\SafetySentinel\Assets\Sentinel.png";
var webPngPath = @"C:\Sentinel\SafetySentinel\WebContent\Sentinel.png";
var testPath = @"C:\Sentinel\SafetySentinel\Assets\test_icon.png";

using var source = new Bitmap(pngPath);
Console.WriteLine($"Source: {source.Width}x{source.Height} ({source.PixelFormat})");

// Ensure square — crop to center if needed
int squareSize = Math.Min(source.Width, source.Height);
int offsetX = (source.Width - squareSize) / 2;
int offsetY = (source.Height - squareSize) / 2;
Console.WriteLine($"Using {squareSize}x{squareSize} from offset ({offsetX},{offsetY})");

using var squareSource = new Bitmap(squareSize, squareSize, PixelFormat.Format32bppArgb);
using (var g = Graphics.FromImage(squareSource))
{
    g.CompositingMode = CompositingMode.SourceOver;
    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    g.DrawImage(source,
        new Rectangle(0, 0, squareSize, squareSize),
        new Rectangle(offsetX, offsetY, squareSize, squareSize),
        GraphicsUnit.Pixel);
}

// Also save a clean 256x256 PNG for WPF Assets and WebContent
using var png256 = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
using (var g = Graphics.FromImage(png256))
{
    g.CompositingMode = CompositingMode.SourceOver;
    g.CompositingQuality = CompositingQuality.HighQuality;
    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    g.SmoothingMode = SmoothingMode.HighQuality;
    g.Clear(Color.Transparent);
    g.DrawImage(squareSource, 0, 0, 256, 256);
}
png256.Save(assetPngPath, ImageFormat.Png);
Console.WriteLine($"Saved Assets PNG: {assetPngPath}");
File.Copy(assetPngPath, webPngPath, true);
Console.WriteLine($"Copied to WebContent: {webPngPath}");

var centerPx = squareSource.GetPixel(squareSize / 2, squareSize / 2);
Console.WriteLine($"Center pixel after crop: ARGB({centerPx.A},{centerPx.R},{centerPx.G},{centerPx.B})");

int[] sizes = { 16, 32, 48, 256 };

using var ms = new MemoryStream();
using var bw = new BinaryWriter(ms);

bw.Write((short)0);
bw.Write((short)1);
bw.Write((short)sizes.Length);

int dataOffset = 6 + (16 * sizes.Length);

var pngDataList = new byte[sizes.Length][];
for (int i = 0; i < sizes.Length; i++)
{
    int sz = sizes[i];
    using var resized = new Bitmap(sz, sz, PixelFormat.Format32bppArgb);
    using (var g = Graphics.FromImage(resized))
    {
        g.CompositingMode = CompositingMode.SourceOver;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.Transparent);
        g.DrawImage(squareSource, 0, 0, sz, sz);
    }

    var px = resized.GetPixel(sz / 2, sz / 2);
    Console.WriteLine($"  {sz}x{sz} center: ARGB({px.A},{px.R},{px.G},{px.B})");

    if (sz == 48)
    {
        resized.Save(testPath, ImageFormat.Png);
        Console.WriteLine($"  Saved test at {testPath}");
    }

    using var pngStream = new MemoryStream();
    resized.Save(pngStream, ImageFormat.Png);
    pngDataList[i] = pngStream.ToArray();
    Console.WriteLine($"  {sz}x{sz}: {pngDataList[i].Length} bytes");
}

for (int i = 0; i < sizes.Length; i++)
{
    bw.Write((byte)(sizes[i] < 256 ? sizes[i] : 0));
    bw.Write((byte)(sizes[i] < 256 ? sizes[i] : 0));
    bw.Write((byte)0);
    bw.Write((byte)0);
    bw.Write((short)1);
    bw.Write((short)32);
    bw.Write((int)pngDataList[i].Length);
    bw.Write(dataOffset);
    dataOffset += pngDataList[i].Length;
}

for (int i = 0; i < sizes.Length; i++)
    bw.Write(pngDataList[i]);

File.WriteAllBytes(icoPath, ms.ToArray());
Console.WriteLine($"\nICO created: {icoPath} ({ms.Length} bytes)");
