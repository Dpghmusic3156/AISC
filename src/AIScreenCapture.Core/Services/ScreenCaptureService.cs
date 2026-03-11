using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace AIScreenCapture.Core.Services;

/// <summary>
/// Handles screen capture using GDI+ with DPI awareness.
/// </summary>
public class ScreenCaptureService
{
    /// <summary>
    /// Captures the entire primary screen as a BitmapSource.
    /// Uses actual pixel dimensions (DPI-aware).
    /// </summary>
    public BitmapSource CaptureFullScreen()
    {
        var screenBounds = GetPrimaryScreenBounds();

        using var bitmap = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);

        return ConvertToBitmapSource(bitmap);
    }

    /// <summary>
    /// Crops a specific region from a full-screen BitmapSource and returns the region as PNG byte array.
    /// The rect coordinates are in actual pixel space.
    /// </summary>
    public byte[] CaptureRegion(BitmapSource fullScreenCapture, Int32Rect region)
    {
        // Clamp region to source dimensions
        int x = Math.Max(0, region.X);
        int y = Math.Max(0, region.Y);
        int width = Math.Min(region.Width, fullScreenCapture.PixelWidth - x);
        int height = Math.Min(region.Height, fullScreenCapture.PixelHeight - y);

        if (width <= 0 || height <= 0)
            throw new ArgumentException("Invalid region: zero or negative size after clamping.");

        var croppedBitmap = new CroppedBitmap(fullScreenCapture, new Int32Rect(x, y, width, height));

        using var stream = new MemoryStream();
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
        encoder.Save(stream);

        return stream.ToArray();
    }

    /// <summary>
    /// Gets the primary screen bounds in physical (device) pixels.
    /// </summary>
    private Rectangle GetPrimaryScreenBounds()
    {
        // Use Win32 to get actual pixel dimensions (DPI-aware)
        int width = GetSystemMetrics(SM_CXSCREEN);
        int height = GetSystemMetrics(SM_CYSCREEN);
        return new Rectangle(0, 0, width, height);
    }

    private static BitmapSource ConvertToBitmapSource(Bitmap bitmap)
    {
        var hBitmap = bitmap.GetHbitmap();
        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            DeleteObject(hBitmap);
        }
    }

    #region P/Invoke

    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr hObject);

    #endregion
}
