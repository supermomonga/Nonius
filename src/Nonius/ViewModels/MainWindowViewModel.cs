using Avalonia.Controls;
using Avalonia.Controls.Notifications;

using Epoxy;
using Epoxy.Synchronized;

using FluentAvalonia.UI.Controls;

using Nonius.Views;

namespace Nonius.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public BindableReactiveProperty<string> WindowTitle { get; } = new();

    public BindableReactiveProperty<ViewModelBase> ContentViewModel { get; } = new();

    public BindableReactiveProperty<NavigationViewItem?> SelectedMenuItem { get; } = new();

    public ReactiveProperty<int> ContentAreaBoundWidth => _Context.ContentAreaBoundWidth;

    public Well<MainWindow> MainWindowWell { get; } = Well.Factory.Create<MainWindow>();

    public Pile<Frame> ContentFramePile { get; } = Pile.Factory.Create<Frame>();

    private readonly ApplicationContextViewModel _Context;
    private readonly MonitoringViewModel _MonitoringViewModel;
    private readonly ProjectsViewModel _ProjectsViewModel;
    private readonly SettingsViewModel _SettingsViewModel;
    private readonly VersionInfoViewModel _VersionInfoViewModel;

    public Pile<WindowNotificationManager> NotificationManagerPile => _Context.NotificationManagerPile;

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        // ViewModels
        _Context = serviceProvider.GetRequiredService<ApplicationContextViewModel>();
        _MonitoringViewModel = serviceProvider.GetRequiredService<MonitoringViewModel>();
        _ProjectsViewModel = serviceProvider.GetRequiredService<ProjectsViewModel>();
        _VersionInfoViewModel = serviceProvider.GetRequiredService<VersionInfoViewModel>();
        _SettingsViewModel = serviceProvider.GetRequiredService<SettingsViewModel>();

        ContentViewModel.Value = _MonitoringViewModel;
        SelectedMenuItem.Subscribe(OnSelectedMenuItemChanged);

        WindowTitle =
            _Context.
            SelectedProject.
            WhereNotNull().
            Select(p => p.Name.AsObservable()).
            Switch().
            Select(name => $"Nonius - {name}").
            ToBindableReactiveProperty(initialValue: "Nonius");

        // Load settings on startup
        MainWindowWell.Add(Control.LoadedEvent, async () => await _Context.LoadSettingsAsync());

        // Re-calulate the width of the profile viewer area when the window size changes
        MainWindowWell.Add(Control.SizeChangedEvent, async () => await ContentFramePile.RentAsync(async (frame) =>
            {
                ContentAreaBoundWidth.Value = (int)frame.Bounds.Width;
                await ValueTask.CompletedTask;
            }));
    }

    private void OnSelectedMenuItemChanged(NavigationViewItem? value)
    {
        switch (value?.Tag)
        {
            case "Monitoring":
                ContentViewModel.Value = _MonitoringViewModel;
                break;
            case "Projects":
                ContentViewModel.Value = _ProjectsViewModel;
                break;
            case "Settings":
                ContentViewModel.Value = _SettingsViewModel;
                break;
            case "VersionInfo":
                ContentViewModel.Value = _VersionInfoViewModel;
                break;
        }
    }
}
