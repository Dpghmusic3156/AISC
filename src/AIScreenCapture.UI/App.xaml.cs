using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using System.Net.Http;
using AIScreenCapture.Core.Models;
using AIScreenCapture.Core.Services;
using AIScreenCapture.UI.Views;

namespace AIScreenCapture.UI;

public partial class App : Application
{
    private TaskbarIcon? _taskbarIcon;
    private ConfigWindow? _configWindow;
    private Window? _hiddenWindow;
    private ResultWindow? _resultWindow;
    private LoadingWindow? _loadingWindow;

    private GlobalInputHook? _globalInputHook;
    private ScreenCaptureService? _screenCaptureService;
    private SettingsManager? _settingsManager;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Create a hidden window to keep the WPF application alive.
            _hiddenWindow = new Window
            {
                Title = "Hidden Tray Host",
                Width = 0,
                Height = 0,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                Visibility = Visibility.Hidden
            };
            _hiddenWindow.Show();

            var icon = System.Drawing.Icon.ExtractAssociatedIcon(
                System.Reflection.Assembly.GetExecutingAssembly().Location);

            _taskbarIcon = new TaskbarIcon
            {
                Icon = icon ?? System.Drawing.SystemIcons.Application,
                ToolTipText = "AI Screen Capture",
                Visibility = Visibility.Visible
            };

            var contextMenu = new ContextMenu();

            var settingsItem = new MenuItem { Header = "Settings..." };
            settingsItem.Click += (s, args) => ShowConfigWindow();

            var captureItem = new MenuItem { Header = "Capture Region" };
            captureItem.Click += (s, args) => TriggerCapture();

            var exitItem = new MenuItem { Header = "Exit" };
            exitItem.Click += delegate
            {
                _taskbarIcon.Dispose();
                Application.Current.Shutdown();
            };

            contextMenu.Items.Add(settingsItem);
            contextMenu.Items.Add(captureItem);
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(exitItem);

            _taskbarIcon.ContextMenu = contextMenu;

            // Initialize services
            _settingsManager = new SettingsManager();
            _screenCaptureService = new ScreenCaptureService();

            // Install global input hooks (Ctrl + Middle Mouse for capture, Middle Mouse for toggle)
            _globalInputHook = new GlobalInputHook();
            _globalInputHook.CaptureTriggered += (s, args) =>
            {
                Dispatcher.Invoke(TriggerCapture);
            };
            _globalInputHook.ToggleResultVisibility += (s, args) =>
            {
                Dispatcher.Invoke(ToggleResultWindow);
            };
            _globalInputHook.Install();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Startup Error: {ex.Message}\n\n{ex.StackTrace}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// Main capture flow: screenshot → overlay → region select → send to AI.
    /// </summary>
    private void TriggerCapture()
    {
        try
        {
            // Step 1: Capture full screen (frozen screenshot)
            var fullScreenshot = _screenCaptureService!.CaptureFullScreen();

            // Step 2: Show the region selection overlay
            var selectionWindow = new RegionSelectionWindow();
            selectionWindow.SetScreenshot(fullScreenshot);
            selectionWindow.PositionFullScreen();

            selectionWindow.RegionSelected += (s, region) =>
            {
                OnRegionSelected(fullScreenshot, region);
            };

            selectionWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Capture Error: {ex.Message}\n\n{ex.StackTrace}",
                "Capture Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Called when the user completes a region selection.
    /// Captures the region and sends it to the active AI preset.
    /// </summary>
    private async void OnRegionSelected(BitmapSource fullScreenshot, Int32Rect region)
    {
        try
        {
            // Capture the selected region as PNG bytes
            byte[] capturedImage = _screenCaptureService!.CaptureRegion(fullScreenshot, region);

            // Load settings and get the active preset
            var settings = _settingsManager!.Load();

            if (settings.Presets.Count == 0)
            {
                MessageBox.Show(
                    "No presets configured. Please add a preset in Settings.",
                    "AI Screen Capture", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int presetIndex = Math.Clamp(settings.ActivePresetIndex, 0, settings.Presets.Count - 1);
            var activePreset = settings.Presets[presetIndex];

            // Get API key for this provider (Custom > Global)
            string apiKey = activePreset.CustomApiKey ?? string.Empty;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = AIServiceFactory.GetApiKey(settings, activePreset.Provider);
            }

            // Determine custom base URL (per-preset override > global setting)
            string? baseUrl = activePreset.CustomBaseUrl;
            if (string.IsNullOrWhiteSpace(baseUrl) && activePreset.Provider == AIProvider.OpenAI)
            {
                baseUrl = settings.BaseUrlOpenAI;
            }

            // Create AI client and send
            var client = AIServiceFactory.CreateClient(activePreset.Provider, apiKey, baseUrl);

            // Hide existing result window while loading
            _resultWindow?.Hide();

            // Show loading window
            if (_loadingWindow == null)
            {
                _loadingWindow = new LoadingWindow();
            }
            _loadingWindow.SetText($"Analyzing with {activePreset.Provider} ({activePreset.ModelName})...");
            _loadingWindow.Show();

            string response = await client.SendImageAsync(
                capturedImage,
                activePreset.SystemPrompt,
                activePreset.ModelName);

            // Hide loading window
            _loadingWindow.Hide();

            // Show result window
            if (_resultWindow == null)
            {
                _resultWindow = new ResultWindow();
            }
            _resultWindow.SetResult(response, activePreset.ModelName);
            _resultWindow.Show();
            _resultWindow.Activate();
        }
        catch (InvalidOperationException ex)
        {
            _loadingWindow?.Hide();
            MessageBox.Show(ex.Message, "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (HttpRequestException ex)
        {
            _loadingWindow?.Hide();
            if (_resultWindow == null) _resultWindow = new ResultWindow();
            _resultWindow.SetResult($"**API Error:**\n\n{ex.Message}", "Error");
            _resultWindow.Show();
        }
        catch (TaskCanceledException)
        {
            _loadingWindow?.Hide();
            if (_resultWindow == null) _resultWindow = new ResultWindow();
            _resultWindow.SetResult("**Timeout (60s)**\n\nThe request took too long. Try a smaller region or check your connection.", "Error");
            _resultWindow.Show();
        }
        catch (Exception ex)
        {
            _loadingWindow?.Hide();
            MessageBox.Show(
                $"Unexpected error: {ex.Message}\n\n{ex.StackTrace}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ToggleResultWindow()
    {
        if (_resultWindow == null) return;

        if (_resultWindow.IsVisible)
        {
            _resultWindow.Hide();
        }
        else
        {
            _resultWindow.Show();
            _resultWindow.Activate();
        }
    }

    private void ShowConfigWindow()
    {
        if (_configWindow == null)
        {
            _configWindow = new ConfigWindow();
            _configWindow.Closed += (s, e) => _configWindow = null;
            _configWindow.Show();
        }
        else
        {
            if (_configWindow.WindowState == WindowState.Minimized)
                _configWindow.WindowState = WindowState.Normal;
            _configWindow.Activate();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _globalInputHook?.Dispose();
        _taskbarIcon?.Dispose();
        base.OnExit(e);
    }
}
