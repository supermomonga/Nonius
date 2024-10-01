using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Nonius.ViewModels;
using Nonius.Views;
using Microsoft.Extensions.Hosting;
using HotAvalonia;
using Avalonia.Preferences;
using WebViewControl;

namespace Nonius;

public partial class App : Application
{
    public IHost? GlobalHost { get; private set; }

    public override void Initialize()
    {
#if DEBUG
        this.EnableHotReload();
#endif
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        // To disable avoid Chromium Safe Storage keychain access
        ((List<KeyValuePair<string, string>>)WebView.Settings.CommandLineSwitches).Add(
            new KeyValuePair<string, string>("use-mock-keychain", string.Empty)
        );
        WebView.Settings.PersistCache = false;
        WebView.Settings.OsrEnabled = false;

        var hostBuilder = CreateHostBuilder();
        var host = hostBuilder.Build();
        GlobalHost = host;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
                DataContext = host.Services.GetRequiredService<MainWindowViewModel>(),
            };
            desktop.Exit += async (sender, e) =>
            {
                await GlobalHost.StopAsync(TimeSpan.FromSeconds(5));
                GlobalHost.Dispose();
                GlobalHost = null;
            };
        }

        base.OnFrameworkInitializationCompleted();

        // Usually, we don't want to block main UI thread.
        // But if it's required to start async services before we create any window,
        // then don't set any MainWindow, and simply call Show() on a new window later after async initialization.
        await host.StartAsync();
    }
    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // ViewModels
                services.AddSingleton<ApplicationContextViewModel>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<MonitoringViewModel>();
                services.AddSingleton<ProjectsViewModel>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<AboutViewModel>();

                services.AddSingleton<Preferences>();
            });
    }
}
