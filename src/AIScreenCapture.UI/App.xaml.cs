using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using System.Net.Http;
using AIScreenCapture.Core.Models;
using AIScreenCapture.Core.Services;
using AIScreenCapture.UI.Views;
using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;

namespace AIScreenCapture.UI;

public partial class App : Application
{
    private TaskbarIcon? _taskbarIcon;
    private ConfigWindow? _configWindow;
    private Window? _hiddenWindow;
    private ResultWindow? _resultWindow;
    private LoadingIndicatorWindow? _loadingIndicatorWindow;
    private bool _isProcessing = false;
    private bool _isSelectingRegion = false;

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

            var aboutItem = new MenuItem { Header = "About" };
            aboutItem.Click += (s, args) => MessageBox.Show("AI Screen Capture\nVersion 1.0\nMade by ghuy", "About", MessageBoxButton.OK, MessageBoxImage.Information);

            var exitItem = new MenuItem { Header = "Exit" };
            exitItem.Click += delegate
            {
                _taskbarIcon.Dispose();
                Application.Current.Shutdown();
            };

            contextMenu.Items.Add(settingsItem);
            contextMenu.Items.Add(captureItem);
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(aboutItem);
            contextMenu.Items.Add(exitItem);

            _taskbarIcon.ContextMenu = contextMenu;

            // Initialize services
            _settingsManager = new SettingsManager();
            _screenCaptureService = new ScreenCaptureService();

            ApplyShortcutConfiguration();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Startup Error: {ex.Message}\n\n{ex.StackTrace}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }

    private void ApplyShortcutConfiguration()
    {
        var settings = _settingsManager!.Load();

        if (settings.UseMouseShortcuts)
        {
            // Clear NHotkey shortcuts
            try { HotkeyManager.Current.Remove("Capture"); } catch { }
            try { HotkeyManager.Current.Remove("Toggle"); } catch { }

            if (_globalInputHook == null)
            {
                _globalInputHook = new GlobalInputHook();
                _globalInputHook.CaptureTriggered += (s, args) => Dispatcher.Invoke(TriggerCapture);
                _globalInputHook.ToggleResultVisibility += (s, args) => Dispatcher.Invoke(ToggleResultWindow);
            }
            _globalInputHook.Install();
        }
        else
        {
            // Remove Mouse shortcuts
            _globalInputHook?.Uninstall();

            // Apply Keyboard shortcuts
            try
            {
                if (TryParseShortcut(settings.CaptureShortcut, out var captureKey, out var captureModifiers))
                {
                    HotkeyManager.Current.AddOrReplace("Capture", captureKey, captureModifiers, (s, e) => Dispatcher.Invoke(TriggerCapture));
                }

                if (TryParseShortcut(settings.ToggleShortcut, out var toggleKey, out var toggleModifiers))
                {
                    HotkeyManager.Current.AddOrReplace("Toggle", toggleKey, toggleModifiers, (s, e) => Dispatcher.Invoke(ToggleResultWindow));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to register keyboard shortcut. It might be in use by another application.\n\n{ex.Message}", "Shortcut Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private bool TryParseShortcut(string shortcutStr, out Key key, out ModifierKeys modifiers)
    {
        key = Key.None;
        modifiers = ModifierKeys.None;

        if (string.IsNullOrWhiteSpace(shortcutStr)) return false;

        var parts = shortcutStr.Split('+');
        if (parts.Length == 0) return false;

        string keyStr = parts[^1]; // Last part is always the key
        if (!Enum.TryParse(keyStr, true, out key))
        {
            return false;
        }

        for (int i = 0; i < parts.Length - 1; i++)
        {
            _ = Enum.TryParse<ModifierKeys>(parts[i], true, out var parsedMod);
            modifiers |= parsedMod;
        }

        return key != Key.None;
    }

    /// <summary>
    /// Main capture flow: screenshot → overlay → region select → send to AI.
    /// </summary>
    private void TriggerCapture()
    {
        if (_isSelectingRegion) return; // Prevent multiple captures opening simultaneously

        try
        {
            _isSelectingRegion = true;

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

            // Reset flags when the selection box is closed/cancelled
            selectionWindow.Closed += (s, args) => 
            {
                _isSelectingRegion = false;
            };

            selectionWindow.Show();
        }
        catch (Exception ex)
        {
            _isSelectingRegion = false;
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

            // Show loading window with timer tracking
            EnsureLoadingWindow();
            _isProcessing = true;
            _loadingIndicatorWindow.StartProcessing();
            _loadingIndicatorWindow.Show();

            string response = await client.SendImageAsync(
                capturedImage,
                activePreset.SystemPrompt,
                activePreset.ModelName);

            // Stop timer and hide loading window
            _isProcessing = false;
            _loadingIndicatorWindow?.StopProcessing();
            _loadingIndicatorWindow?.Hide();

            // Show result window
            EnsureResultWindow();
            _resultWindow.SetResult(response, activePreset.ModelName);
            _resultWindow.Show();
            _resultWindow.Activate();
        }
        catch (InvalidOperationException ex)
        {
            _isProcessing = false;
            _loadingIndicatorWindow?.StopProcessing();
            _loadingIndicatorWindow?.Hide();
            if (_resultWindow == null) EnsureResultWindow();
            _resultWindow.SetResult($"**Configuration Error:**\n\n{ex.Message}", "Error");
            _resultWindow.Show();
        }
        catch (HttpRequestException ex)
        {
            _isProcessing = false;
            _loadingIndicatorWindow?.StopProcessing();
            _loadingIndicatorWindow?.Hide();
            if (_resultWindow == null) EnsureResultWindow();
            _resultWindow.SetResult($"**API Error:**\n\n{ex.Message}", "Error");
            _resultWindow.Show();
        }
        catch (TaskCanceledException)
        {
            _isProcessing = false;
            _loadingIndicatorWindow?.StopProcessing();
            _loadingIndicatorWindow?.Hide();
            if (_resultWindow == null) EnsureResultWindow();
            _resultWindow.SetResult("**Timeout (60s)**\n\nThe request took too long. Try a smaller region or check your connection.", "Error");
            _resultWindow.Show();
        }
        catch (Exception ex)
        {
            _isProcessing = false;
            _loadingIndicatorWindow?.StopProcessing();
            _loadingIndicatorWindow?.Hide();
            if (_resultWindow == null) EnsureResultWindow();
            _resultWindow.SetResult($"**Unexpected Error:**\n\n{ex.Message}\n\n```\n{ex.StackTrace}\n```", "Error");
            _resultWindow.Show();
        }
    }

    private void ToggleResultWindow()
    {
        if (_isProcessing && _loadingIndicatorWindow != null)
        {
            if (_loadingIndicatorWindow.IsVisible)
            {
                _loadingIndicatorWindow.Hide();
            }
            else
            {
                _loadingIndicatorWindow.Show();
            }
            return;
        }

        if (_resultWindow == null) return;

        if (_resultWindow.IsVisible)
        {
            try { _resultWindow.Hide(); } catch { }
        }
        else
        {
            try { _resultWindow.Show(); _resultWindow.Activate(); } catch { }
        }
    }

    private void EnsureResultWindow()
    {
        if (_resultWindow == null)
        {
            _resultWindow = new ResultWindow();
            _resultWindow.Closed += (s, e) => _resultWindow = null;
        }
    }

    private void EnsureLoadingWindow()
    {
        if (_loadingIndicatorWindow == null)
        {
            _loadingIndicatorWindow = new LoadingIndicatorWindow();
            _loadingIndicatorWindow.Closed += (s, e) => _loadingIndicatorWindow = null;
        }
    }

    private void ShowConfigWindow()
    {
        if (_configWindow == null)
        {
            _configWindow = new ConfigWindow();
            _configWindow.Closed += (s, e) => 
            {
                _configWindow = null;
                ApplyShortcutConfiguration(); // Re-apply if user changed keybinds
            };
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
