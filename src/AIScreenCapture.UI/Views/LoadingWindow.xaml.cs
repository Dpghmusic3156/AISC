using System.Windows;
using System.Windows.Input;
using AIScreenCapture.Core.Models;
using AIScreenCapture.Core.Services;

namespace AIScreenCapture.UI.Views;

public partial class LoadingWindow : Window
{
    private readonly SettingsManager _settingsManager;
    private readonly AppSettings _settings;
    private bool _settingsLoaded = false;

    public LoadingWindow()
    {
        InitializeComponent();
        
        _settingsManager = new SettingsManager();
        _settings = _settingsManager.Load();
        
        Loaded += LoadingWindow_Loaded;
        LocationChanged += LoadingWindow_LocationChanged;
    }

    private void LoadingWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Restore previous dimensions entirely from AppSettings
        if (_settings.LoadingWindowLeft >= 0 && _settings.LoadingWindowTop >= 0)
        {
            this.Left = _settings.LoadingWindowLeft;
            this.Top = _settings.LoadingWindowTop;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        _settingsLoaded = true;
    }

    private void LoadingWindow_LocationChanged(object? sender, EventArgs e)
    {
        if (!_settingsLoaded) return;
        
        if (WindowState == WindowState.Normal)
        {
            _settings.LoadingWindowLeft = this.Left;
            _settings.LoadingWindowTop = this.Top;
            _settingsManager.Save(_settings);
        }
    }
    
    public void SetText(string text)
    {
        LoadingText.Text = text;
    }

    private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}
