using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using H.NotifyIcon;

namespace AIScreenCapture.UI;

public partial class App : Application
{
    private TaskbarIcon? _taskbarIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _taskbarIcon = new TaskbarIcon
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location),
            ToolTipText = "AI Screen Capture"
        };
        
        var contextMenu = new ContextMenu();
        
        var settingsItem = new MenuItem { Header = "Settings...", IsEnabled = false };
        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (s, args) => Application.Current.Shutdown();

        contextMenu.Items.Add(settingsItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(exitItem);

        _taskbarIcon.ContextMenu = contextMenu;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _taskbarIcon?.Dispose();
        base.OnExit(e);
    }
}

