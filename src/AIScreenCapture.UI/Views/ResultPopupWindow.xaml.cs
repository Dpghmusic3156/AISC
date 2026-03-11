using System.Windows;
using System.Windows.Input;

namespace AIScreenCapture.UI.Views;

public partial class ResultPopupWindow : Window
{
    public ResultPopupWindow()
    {
        InitializeComponent();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            this.DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Hide(); 
        // We hide instead of close so that we can reuse the same popup instance later
    }
}
