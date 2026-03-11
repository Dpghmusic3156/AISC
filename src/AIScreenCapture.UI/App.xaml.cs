using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using H.NotifyIcon;
using AIScreenCapture.UI.Views;

namespace AIScreenCapture.UI;

public partial class App : Application
{
    private TaskbarIcon? _taskbarIcon;
    private ConfigWindow? _configWindow;
    private ResultPopupWindow? _resultPopupWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _taskbarIcon = new TaskbarIcon
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location),
            ToolTipText = "AI Screen Capture"
        };
        
        var contextMenu = new ContextMenu();
        
        var settingsItem = new MenuItem { Header = "Settings..." };
        settingsItem.Click += (s, args) => ShowConfigWindow();

        var testPopupItem = new MenuItem { Header = "Test Popup" };
        testPopupItem.Click += (s, args) => ShowResultPopupWindow();

        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (s, args) => Application.Current.Shutdown();

        contextMenu.Items.Add(settingsItem);
        contextMenu.Items.Add(testPopupItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(exitItem);

        _taskbarIcon.ContextMenu = contextMenu;
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

    private void ShowResultPopupWindow()
    {
        if (_resultPopupWindow == null)
        {
            _resultPopupWindow = new ResultPopupWindow();
            _resultPopupWindow.Closed += (s, e) => _resultPopupWindow = null;
        }
        _resultPopupWindow.Show();
        _resultPopupWindow.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _taskbarIcon?.Dispose();
        base.OnExit(e);
    }
}

