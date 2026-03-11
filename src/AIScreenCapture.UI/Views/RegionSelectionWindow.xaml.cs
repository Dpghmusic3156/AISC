using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AIScreenCapture.Core.Services;

namespace AIScreenCapture.UI.Views;

public partial class RegionSelectionWindow : Window
{
    private Point _startPoint;
    private Rectangle? _selectionRect;
    private bool _isDragging;
    private BitmapSource? _screenshot;

    /// <summary>
    /// Fired when user completes a region selection. Returns the selected rectangle in pixel coordinates.
    /// </summary>
    public event EventHandler<Int32Rect>? RegionSelected;

    /// <summary>
    /// Fired when user cancels the selection (Escape or right-click).
    /// </summary>
    public event EventHandler? SelectionCancelled;

    public RegionSelectionWindow()
    {
        InitializeComponent();
        
        // Apply opacity from settings
        var settingsManager = new SettingsManager();
        var settings = settingsManager.Load();
        
        byte alpha = (byte)(settings.SelectionOpacity * 255);
        OverlayCanvas.Background = new SolidColorBrush(Color.FromArgb(alpha, 0, 0, 0));
    }

    /// <summary>
    /// Sets the frozen screenshot as the background of the overlay.
    /// Must be called before showing the window.
    /// </summary>
    public void SetScreenshot(BitmapSource screenshot)
    {
        _screenshot = screenshot;
        ScreenshotBackground.Source = screenshot;
    }

    /// <summary>
    /// Positions the window to cover the full primary screen using WPF DIPs.
    /// </summary>
    public void PositionFullScreen()
    {
        // Use primary screen dimensions in DIPs (not VirtualScreen which spans all monitors)
        Left = 0;
        Top = 0;
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(OverlayCanvas);
        _isDragging = true;

        // Remove any existing selection rectangle
        if (_selectionRect != null)
        {
            OverlayCanvas.Children.Remove(_selectionRect);
        }

        // Create a new selection rectangle
        _selectionRect = new Rectangle
        {
            Stroke = Brushes.White,
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 4, 2 },
            Fill = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
            Width = 0,
            Height = 0
        };

        Canvas.SetLeft(_selectionRect, _startPoint.X);
        Canvas.SetTop(_selectionRect, _startPoint.Y);
        OverlayCanvas.Children.Add(_selectionRect);

        OverlayCanvas.CaptureMouse();
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _selectionRect == null)
            return;

        var currentPoint = e.GetPosition(OverlayCanvas);

        double x = Math.Min(_startPoint.X, currentPoint.X);
        double y = Math.Min(_startPoint.Y, currentPoint.Y);
        double width = Math.Abs(currentPoint.X - _startPoint.X);
        double height = Math.Abs(currentPoint.Y - _startPoint.Y);

        Canvas.SetLeft(_selectionRect, x);
        Canvas.SetTop(_selectionRect, y);
        _selectionRect.Width = width;
        _selectionRect.Height = height;
    }

    private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging || _selectionRect == null || _screenshot == null)
            return;

        _isDragging = false;
        OverlayCanvas.ReleaseMouseCapture();

        var currentPoint = e.GetPosition(OverlayCanvas);

        double x = Math.Min(_startPoint.X, currentPoint.X);
        double y = Math.Min(_startPoint.Y, currentPoint.Y);
        double width = Math.Abs(currentPoint.X - _startPoint.X);
        double height = Math.Abs(currentPoint.Y - _startPoint.Y);

        // Minimum selection size (ignore accidental clicks)
        if (width < 10 || height < 10)
        {
            OverlayCanvas.Children.Remove(_selectionRect);
            _selectionRect = null;
            return;
        }

        // Convert DIP coordinates to screenshot pixel coordinates using ratio mapping.
        // This avoids DPI mismatch issues — we map from canvas DIP space to screenshot pixel space.
        double canvasWidth = OverlayCanvas.ActualWidth;
        double canvasHeight = OverlayCanvas.ActualHeight;
        double scaleX = _screenshot.PixelWidth / canvasWidth;
        double scaleY = _screenshot.PixelHeight / canvasHeight;

        int pixelX = Math.Max(0, (int)(x * scaleX));
        int pixelY = Math.Max(0, (int)(y * scaleY));
        int pixelW = (int)(width * scaleX);
        int pixelH = (int)(height * scaleY);

        // Clamp to screenshot bounds
        pixelW = Math.Min(pixelW, _screenshot.PixelWidth - pixelX);
        pixelH = Math.Min(pixelH, _screenshot.PixelHeight - pixelY);

        if (pixelW <= 0 || pixelH <= 0)
        {
            OverlayCanvas.Children.Remove(_selectionRect);
            _selectionRect = null;
            return;
        }

        var pixelRect = new Int32Rect(pixelX, pixelY, pixelW, pixelH);

        Close();
        RegionSelected?.Invoke(this, pixelRect);
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _isDragging = false;
            Close();
            SelectionCancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        Close();
        SelectionCancelled?.Invoke(this, EventArgs.Empty);
    }
}
