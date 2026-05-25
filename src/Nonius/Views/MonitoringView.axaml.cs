using Avalonia.Controls;
using Avalonia.Platform;

namespace Nonius.Views;

public partial class MonitoringView : UserControl
{
    public MonitoringView()
    {
        InitializeComponent();
    }

    private void NativeWebView_OnEnvironmentRequested(object? sender, WebViewEnvironmentRequestedEventArgs e)
    {
        e.EnableDevTools = true;

        if (e is AppleWKWebViewEnvironmentRequestedEventArgs wkWebView)
        {
            wkWebView.NonPersistentDataStore = true;
            wkWebView.UpgradeKnownHostsToHTTPS = false;
        }
        else if (e is WindowsWebView2EnvironmentRequestedEventArgs webView2)
        {
            webView2.IsInPrivateModeEnabled = true;
        }
    }
}
