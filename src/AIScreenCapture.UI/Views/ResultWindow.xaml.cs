using System.Windows;
using System.Windows.Input;
using AIScreenCapture.Core.Models;
using AIScreenCapture.Core.Services;

namespace AIScreenCapture.UI.Views;

public partial class ResultWindow : Window
{
    private string _rawMarkdown = string.Empty;
    private readonly SettingsManager _settingsManager;
    private readonly AppSettings _settings;
    private bool _settingsLoaded = false;

    public ResultWindow()
    {
        InitializeComponent();
        
        _settingsManager = new SettingsManager();
        _settings = _settingsManager.Load();
        
        Loaded += ResultWindow_Loaded;
        SizeChanged += ResultWindow_SizeOrLocationChanged;
        LocationChanged += ResultWindow_SizeOrLocationChanged;
    }

    private void ResultWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Restore previous dimensions entirely from AppSettings
        if (_settings.ResultWindowWidth > 0 && _settings.ResultWindowHeight > 0)
        {
            this.Width = _settings.ResultWindowWidth;
            this.Height = _settings.ResultWindowHeight;
            
            // Validate screen bounds before restoring coordinates
            if (_settings.ResultWindowLeft >= 0 && _settings.ResultWindowTop >= 0)
            {
                this.Left = _settings.ResultWindowLeft;
                this.Top = _settings.ResultWindowTop;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        _settingsLoaded = true;
    }

    private void ResultWindow_SizeOrLocationChanged(object? sender, EventArgs e)
    {
        if (!_settingsLoaded) return;
        
        // Save bounds when changed
        if (WindowState == WindowState.Normal)
        {
            _settings.ResultWindowWidth = this.Width;
            _settings.ResultWindowHeight = this.Height;
            _settings.ResultWindowLeft = this.Left;
            _settings.ResultWindowTop = this.Top;
            
            _settingsManager.Save(_settings);
        }
    }

    public void SetResult(string markdown, string modelName)
    {
        _rawMarkdown = markdown;
        TitleText.Text = $"AI Result ({modelName})";
        
        // MdXaml MarkdownScrollViewer rendering
        MarkdownViewer.Markdown = markdown;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide(); // Hide instead of closing so we maintain state during the session if needed
    }

    private async void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_rawMarkdown);
        
        // Flash confirmation
        NotificationText.Visibility = Visibility.Visible;
        await Task.Delay(2000);
        NotificationText.Visibility = Visibility.Hidden;
    }
}
