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
    public Pile<MainWindow> MainWindowPile { get; } = Pile.Factory.Create<MainWindow>();
    public Well<TextBlock> WindowTitleTextBlockWell { get; } = Well.Factory.Create<TextBlock>();
    public Pile<TextBlock> WindowTitleTextBlockPile { get; } = Pile.Factory.Create<TextBlock>();

    public Pile<Frame> ContentFramePile { get; } = Pile.Factory.Create<Frame>();

    private readonly ApplicationContextViewModel _Context;
    private readonly MonitoringViewModel _MonitoringViewModel;
    private readonly ProjectsViewModel _ProjectsViewModel;
    private readonly SettingsViewModel _SettingsViewModel;
    private readonly AboutViewModel _AboutViewModel;

    public Pile<WindowNotificationManager> NotificationManagerPile => _Context.NotificationManagerPile;

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        // ViewModels
        _Context = serviceProvider.GetRequiredService<ApplicationContextViewModel>();
        _MonitoringViewModel = serviceProvider.GetRequiredService<MonitoringViewModel>();
        _ProjectsViewModel = serviceProvider.GetRequiredService<ProjectsViewModel>();
        _AboutViewModel = serviceProvider.GetRequiredService<AboutViewModel>();
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

        // Drag window by controls placed on the title bar
        WindowTitleTextBlockWell.Add(Control.PointerPressedEvent, ev =>
        {
            WindowTitleTextBlockPile.RentSync(tb =>
            {
                if (ev.GetCurrentPoint(tb).Properties.IsLeftButtonPressed)
                {
                    MainWindowPile.RentSync(win => win.BeginMoveDrag(ev));
                }
            });
            return ValueTask.CompletedTask;
        });
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
            case "About":
                ContentViewModel.Value = _AboutViewModel;
                break;
        }
    }
}
