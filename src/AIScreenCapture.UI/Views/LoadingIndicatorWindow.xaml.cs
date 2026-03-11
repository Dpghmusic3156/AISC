using System;
using System.Windows;
using System.Windows.Threading;
using AIScreenCapture.Core.Models;
using AIScreenCapture.Core.Services;

namespace AIScreenCapture.UI.Views;

public partial class LoadingIndicatorWindow : Window
{
    private readonly SettingsManager _settingsManager;
    private readonly AppSettings _settings;
    private DispatcherTimer? _timer;
    private int _elapsedSeconds = 0;

    public LoadingIndicatorWindow()
    {
        InitializeComponent();
        
        _settingsManager = new SettingsManager();
        _settings = _settingsManager.Load();
        
        Loaded += LoadingIndicatorWindow_Loaded;
        SizeChanged += LoadingIndicatorWindow_SizeChanged;
    }

    private void LoadingIndicatorWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Apply scale if needed
        var checkmarkScale = _settings.CheckmarkScale > 0 ? _settings.CheckmarkScale : 1.0;
        var scaleTransform = new System.Windows.Media.ScaleTransform(checkmarkScale, checkmarkScale);
        this.LayoutTransform = scaleTransform;

        ApplyOverlayPosition();
    }

    private void LoadingIndicatorWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (IsLoaded && Visibility == Visibility.Visible)
        {
            ApplyOverlayPosition();
        }
    }

    private void ApplyOverlayPosition()
    {
        // Position according to global settings
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        var margin = 12;

        var actualW = double.IsNaN(this.ActualWidth) || this.ActualWidth == 0 ? 30 : this.ActualWidth;
        var actualH = double.IsNaN(this.ActualHeight) || this.ActualHeight == 0 ? 30 : this.ActualHeight;

        switch (_settings.OverlayPosition)
        {
            case OverlayPosition.BottomRight:
                Left = screenWidth - actualW - margin;
                Top = screenHeight - actualH - margin;
                break;
            case OverlayPosition.BottomLeft:
                Left = margin;
                Top = screenHeight - actualH - margin;
                break;
            case OverlayPosition.TopRight:
                Left = screenWidth - actualW - margin;
                Top = margin;
                break;
            case OverlayPosition.TopLeft:
                Left = margin;
                Top = margin;
                break;
            case OverlayPosition.Center:
            default:
                Left = (screenWidth - actualW) / 2;
                Top = (screenHeight - actualH) / 2;
                break;
        }
    }

    public void StartProcessing()
    {
        TimerText.Visibility = Visibility.Collapsed;
    }

    public void StopProcessing()
    {
        _timer?.Stop();
    }

    protected override void OnPreviewMouseUp(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnPreviewMouseUp(e);

        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        {
            this.Close();
            e.Handled = true;
        }
        else if (e.ChangedButton == System.Windows.Input.MouseButton.Middle)
        {
            this.Hide();
            e.Handled = true;
        }
    }
}
