using CommunityToolkit.Mvvm.Input;

using Avalonia.Threading;
using Epoxy;
using Epoxy.Synchronized;
using Nonius.Models.Vernier;
using Nonius.Services;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Threading;

namespace Nonius.ViewModels;

public partial class MonitoringViewModel : ViewModelBase
{
    private readonly ApplicationContextViewModel _Context;
    private readonly ProfileViewerServer _ProfileViewerServer;
    public BindableReactiveProperty<string> LogDirectoryPath { get; }
    public Observable<bool> IsValidLogDirectoryPath { get; }

    public ObservableCollection<ProfilingDataItemViewModel> ProfilingDataList { get; } = [];
    public ReadOnlyObservableCollection<ProfilingDataItemViewModel> ProfilingDataListView { get; }

    public INotifyCollectionChangedSynchronizedViewList<ProjectViewModel> ProjectsView { get; }
    public BindableReactiveProperty<ProjectViewModel?> SelectedProject => _Context.SelectedProject;
    public BindableReactiveProperty<TimeSpan> DisplayDuration => _Context.DisplayDuration;
    public BindableReactiveProperty<ProfilingDataItemViewModel> SelectedProfilingData { get; } = new();
    public BindableReactiveProperty<bool> ShowProfileViewer { get; } = new(false);
    public BindableReactiveProperty<int> ViewerPaneWidth => _Context.ViewerPaneWidth;
    public BindableReactiveProperty<Uri> BrowserAddress { get; } = new(new Uri("about:blank"));

    public Pile<NativeWebView> WebViewPile { get; } = Pile.Factory.Create<NativeWebView>();
    public Well<SplitView> SplitViewWell { get; } = Well.Factory.Create<SplitView>();
    public Well<UserControl> ContainerWell { get; } = Well.Factory.Create<UserControl>();

    private FileSystemWatcher? _Watcher;
    public MonitoringViewModel(ApplicationContextViewModel context, ProfileViewerServer profileViewerServer)
    {
        System.Console.WriteLine($"Thread: {Environment.CurrentManagedThreadId}");
        _Context = context;
        _ProfileViewerServer = profileViewerServer;
        ProjectsView = _Context.Projects.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

        ProfilingDataListView = new ReadOnlyObservableCollection<ProfilingDataItemViewModel>(ProfilingDataList);
        LogDirectoryPath = SelectedProject.WhereNotNull().Select(x => x.ProfileDataDir.AsObservable()).Switch().ToBindableReactiveProperty(string.Empty);
        IsValidLogDirectoryPath = LogDirectoryPath.Select(Directory.Exists);
        LogDirectoryPath.SubscribeAwait(async (path, ct) => await OnLogDirectoryPathChangedAsync(path, ct));

        SelectedProfilingData.Subscribe(x =>
        {
            if (x == null)
            {
                ShowProfileViewer.Value = false;
            }
            else
            {
                ShowProfileViewer.Value = true;
                Uri url = _ProfileViewerServer.GetProfileViewerUri(x.DataFilePath);
                if (BrowserAddress.Value != url)
                {
                    BrowserAddress.Value = url;
                }
            }
        });

        SplitViewWell.Add(SplitView.PaneClosingEvent, (e) =>
        {
            // 勝手に閉じようとするのを防ぐ
            e.Cancel = ShowProfileViewer.Value;
            return ValueTask.CompletedTask;
        });
    }

    private async Task InvalidateProfilingDataListAsync(string path, CancellationToken ct = default)
    {
        IReadOnlyList<ProfilingDataItemViewModel> items = await ProfilingDataItemLoader.LoadAsync(path, ct);
        await RunOnUIThreadAsync(() =>
        {
            foreach (ProfilingDataItemViewModel item in items)
            {
                if (!ProfilingDataList.Contains(item))
                {
                    ProfilingDataList.Add(item);
                }
            }
        });
    }

    private async Task OnLogDirectoryPathChangedAsync(string path, CancellationToken ct)
    {
        await RunOnUIThreadAsync(ProfilingDataList.Clear);
        InvalidateLogDirectoryWatcher(path);
        await InvalidateProfilingDataListAsync(path, ct);
    }

    private void InvalidateLogDirectoryWatcher(string path)
    {
        _Watcher?.Dispose();
        if (Directory.Exists(path))
        {
            _Watcher = new FileSystemWatcher(path, "*.json")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
                NotifyFilter =
                    NotifyFilters.Attributes |
                    NotifyFilters.CreationTime |
                    NotifyFilters.DirectoryName |
                    NotifyFilters.FileName |
                    NotifyFilters.LastAccess |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Security |
                    NotifyFilters.Size
            };
            _Watcher.Created += OnProfileDataCreated;
            _Watcher.Deleted += OnProfileDataDeleted;
            _Watcher.Renamed += OnProfileDataRenamed;
        }
    }

    private void OnProfileDataCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            using FileStream stream = File.OpenRead(e.FullPath);
            ProfilingData? data = JsonSerializer.Deserialize<ProfilingData>(stream);
            if (data != null)
            {
                ProfilingDataItemViewModel item = new()
                {
                    DataFilePath = e.FullPath,
                    Data = data
                };
                RunOnUIThread(() =>
                {
                    if (!ProfilingDataList.Contains(item))
                    {
                        ProfilingDataList.Insert(0, item);
                    }
                });
            }
        }
        catch { }
    }

    private void OnProfileDataDeleted(object sender, FileSystemEventArgs e)
    {
        RunOnUIThread(() =>
        {
            var item = ProfilingDataList.FirstOrDefault(x => x.DataFilePath == e.FullPath);
            if (item != null)
            {
                ProfilingDataList.Remove(item);
            }
        });
    }

    private void OnProfileDataRenamed(object sender, RenamedEventArgs e)
    {
        RunOnUIThread(() =>
        {
            var item = ProfilingDataList.FirstOrDefault(x => x.DataFilePath == e.OldFullPath);
            if (item != null)
            {
                item.DataFilePath = e.FullPath;
            }
        });
    }

    private static async Task RunOnUIThreadAsync(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(action);
        }
    }

    private static void RunOnUIThread(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
        }
        else
        {
            Dispatcher.UIThread.Post(action);
        }
    }

    [RelayCommand]
    public void Reload()
        => WebViewPile.RentSync(webView => webView.Refresh());
}