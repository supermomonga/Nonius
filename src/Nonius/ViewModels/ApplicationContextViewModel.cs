using Avalonia.Controls.Notifications;
using Avalonia.Preferences;

using Epoxy;

using Nonius.Models.Settings;

namespace Nonius.ViewModels;

public partial class ApplicationContextViewModel : ViewModelBase
{
    public object SyncRoot { get; } = new();

    private Preferences Preferences { get; }
    public BindableReactiveProperty<ProjectViewModel?> SelectedProject { get; } = new();
    public BindableReactiveProperty<int> ViewerPaneWidthRatio { get; } = new();
    public BindableReactiveProperty<int> ContentAreaBoundWidth { get; } = new();
    public BindableReactiveProperty<int> ViewerPaneWidth { get; }
    public BindableReactiveProperty<TimeSpan> DisplayDuration { get; } = new();
    public ObservableList<ProjectViewModel> Projects { get; } = [];
    public Pile<WindowNotificationManager> NotificationManagerPile { get; } = Pile.Factory.Create<WindowNotificationManager>();

    public ApplicationContextViewModel(Preferences preferences)
    {
        Preferences = preferences;
        ViewerPaneWidth = ViewerPaneWidthRatio.CombineLatest(ContentAreaBoundWidth, (ratio, width) => (int)(width * ratio / 100.0)).ToBindableReactiveProperty();

        SelectedProject.Subscribe(async v =>
        {
            if (v != null)
            {
                await SaveSettingsAsync();
            }
        });
    }

    public async Task ShowNotificationAsync(string? title, string? message)
    {

        await NotificationManagerPile.RentAsync(async manager =>
        {
            manager.Show(new Notification(title, message));
            await ValueTask.CompletedTask;
        });
    }

    public async Task LoadSettingsAsync(bool showNotification = false)
    {
        // TODO: Use SyncRoot

        ViewerPaneWidthRatio.Value = await Preferences.GetAsync(nameof(ViewerPaneWidthRatio), 80);

        var projects = await Preferences.GetAsync(nameof(Projects), new List<ProjectSetting>());
        if (projects != null && projects.Count != 0)
        {
            lock (Projects.SyncRoot)
            {
                var vms = projects.Select(p => new ProjectViewModel(
                    id: p.Id,
                    name: p.Name,
                    profileDataDir: p.ProfileDataDir,
                    sourceCodeDir: p.SourceCodeDir
                ));
                Projects.Clear();
                Projects.AddRange(vms);
            }
        }
        DisplayDuration.Value = await Preferences.GetAsync(nameof(DisplayDuration), TimeSpan.FromMinutes(15));

        var selectedProjectId = await Preferences.GetAsync(nameof(SelectedProject), Guid.Empty);
        SelectedProject.Value = Projects.FirstOrDefault(p => selectedProjectId.Equals(p?.Id), Projects.FirstOrDefault());

        if (showNotification)
        {
            await ShowNotificationAsync("Settings Loaded", "Settings have been loaded.");
        }
    }

    public async Task SaveSettingsAsync(bool showNotification = false)
    {
        // TODO: Use SyncRoot

        await Task.WhenAll(
            Preferences.SetAsync(nameof(SelectedProject), SelectedProject.Value?.Id),
            Preferences.SetAsync(nameof(ViewerPaneWidthRatio), ViewerPaneWidthRatio.Value),
            Preferences.SetAsync(nameof(DisplayDuration), DisplayDuration.Value)
        );

        List<ProjectSetting> projects;
        lock (Projects.SyncRoot)
        {
            projects = Projects.Select(vm => new ProjectSetting()
            {
                Id = vm.Id,
                Name = vm.Name.Value,
                ProfileDataDir = vm.ProfileDataDir.Value,
                SourceCodeDir = vm.SourceCodeDir.Value,
            }).ToList();
        }
        if (projects.Count != 0)
        {
            await Preferences.SetAsync(nameof(Projects), projects);
        }

        if (showNotification)
        {
            await ShowNotificationAsync("Settings Saved", "Settings have been saved.");
        }
    }
}
