using System.Windows;

namespace AIScreenCapture.UI.Views;

public partial class ConfigWindow : Window
{
    public ConfigWindow()
    {
        InitializeComponent();
        
        if (DataContext is ViewModels.ConfigViewModel vm)
        {
            vm.RequestClose = new Action(() => this.Close());
        }
    }

    private void HotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        e.Handled = true;

        var key = e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key;
        
        // Ignore modifier keys only
        if (key == System.Windows.Input.Key.LeftShift || key == System.Windows.Input.Key.RightShift ||
            key == System.Windows.Input.Key.LeftCtrl || key == System.Windows.Input.Key.RightCtrl ||
            key == System.Windows.Input.Key.LeftAlt || key == System.Windows.Input.Key.RightAlt ||
            key == System.Windows.Input.Key.LWin || key == System.Windows.Input.Key.RWin)
        {
            return;
        }

        var modifiers = System.Windows.Input.Keyboard.Modifiers;
        string shortcut = "";

        if (modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control)) shortcut += "Control+";
        if (modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift)) shortcut += "Shift+";
        if (modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt)) shortcut += "Alt+";
        if (modifiers.HasFlag(System.Windows.Input.ModifierKeys.Windows)) shortcut += "Windows+";

        shortcut += key.ToString();

        if (sender is System.Windows.Controls.TextBox tb)
        {
            tb.Text = shortcut;

            // Force update binding since we intercepted it
            var binding = tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            binding?.UpdateSource();
        }
    }
}
