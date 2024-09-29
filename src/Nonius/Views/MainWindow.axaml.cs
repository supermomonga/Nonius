using Avalonia.Controls;

using FluentAvalonia.UI.Windowing;

using WebViewControl;

namespace Nonius.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        WebView.Settings.OsrEnabled = false;
        InitializeComponent();
    }
}
