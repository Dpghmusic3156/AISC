using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AIScreenCapture.Core.Models;
using AIScreenCapture.Core.Services;

namespace AIScreenCapture.UI.Views;

public partial class ResultWindow : Window
{
    private string _rawMarkdown = string.Empty;
    private readonly SettingsManager _settingsManager;
    private readonly AppSettings _settings;

    public ResultWindow()
    {
        InitializeComponent();
        
        _settingsManager = new SettingsManager();
        _settings = _settingsManager.Load();
        
        Loaded += ResultWindow_Loaded;
        SizeChanged += ResultWindow_SizeChanged;
    }

    private void ResultWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyAppearanceSettings();
        ApplyOverlayPosition();
    }

    private void ApplyAppearanceSettings()
    {
        this.Opacity = _settings.Opacity;
        WindowScale.ScaleX = _settings.Scale;
        WindowScale.ScaleY = _settings.Scale;
        
        if (_settings.FontSize > 0)
        {
            ResultText.FontSize = _settings.FontSize;
        }

        if (_settings.Theme == AppTheme.Light)
        {
            MainBorder.Background = new SolidColorBrush(Color.FromArgb(242, 255, 255, 255)); // rgba(255, 255, 255, 0.95)
            ResultText.Foreground = new SolidColorBrush(Color.FromRgb(31, 41, 55)); // #1f2937
        }
        else
        {
            MainBorder.Background = new SolidColorBrush(Color.FromArgb(242, 31, 41, 55)); // rgba(31, 41, 55, 0.95)
            ResultText.Foreground = new SolidColorBrush(Color.FromRgb(243, 244, 246)); // #f3f4f6
            MainBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(55, 65, 81)); // #374151
            MainBorder.BorderThickness = new Thickness(1);
        }
    }

    private void ApplyOverlayPosition()
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        var margin = 12;

        // Use Actual sizes if available (since SizeToContent sets Width/Height to NaN)
        var actualW = double.IsNaN(this.ActualWidth) || this.ActualWidth == 0 ? 300 : this.ActualWidth;
        var actualH = double.IsNaN(this.ActualHeight) || this.ActualHeight == 0 ? 100 : this.ActualHeight;

        switch (_settings.OverlayPosition)
        {
            case OverlayPosition.BottomRight:
                this.Left = screenWidth - actualW - margin;
                this.Top = screenHeight - actualH - margin;
                break;
            case OverlayPosition.BottomLeft:
                this.Left = margin;
                this.Top = screenHeight - actualH - margin;
                break;
            case OverlayPosition.TopRight:
                this.Left = screenWidth - actualW - margin;
                this.Top = margin;
                break;
            case OverlayPosition.TopLeft:
                this.Left = margin;
                this.Top = margin;
                break;
            case OverlayPosition.Center:
            default:
                this.Left = (screenWidth - actualW) / 2;
                this.Top = (screenHeight - actualH) / 2;
                break;
        }
    }

    private void ResultWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (IsLoaded && Visibility == Visibility.Visible)
        {
            ApplyOverlayPosition();
        }
    }

    public void SetResult(string markdown, string modelName)
    {
        _rawMarkdown = markdown;
        
        ResultText.Text = markdown.Trim();

        // Apply FontSize
        if (_settings.FontSize > 0)
        {
            ResultText.FontSize = _settings.FontSize;
        }

        // Force layout update so ActualWidth/ActualHeight calculate, then reposition
        this.UpdateLayout();
        ApplyOverlayPosition();
    }

    protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseUp(e);

        if (e.ChangedButton == MouseButton.Left)
        {
            this.Close();
            e.Handled = true;
        }
        else if (e.ChangedButton == MouseButton.Middle)
        {
            this.Hide();
            e.Handled = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_rawMarkdown);
        
        // Flash confirmation
        NotificationText.Visibility = Visibility.Visible;
        await Task.Delay(2000);
        NotificationText.Visibility = Visibility.Collapsed;
    }
}
