using CommunityToolkit.Mvvm.Input;

using Epoxy;
using Epoxy.Synchronized;
using Nonius.Models.Vernier;
using Nonius.Services;
using Avalonia.Controls;
using System.Threading;

namespace Nonius.ViewModels;

public partial class MonitoringViewModel : ViewModelBase
{
    private readonly ApplicationContextViewModel _Context;
    private readonly ProfileViewerServer _ProfileViewerServer;
    public BindableReactiveProperty<string> LogDirectoryPath { get; }
    public Observable<bool> IsValidLogDirectoryPath { get; }

    public ObservableList<ProfilingDataItemViewModel> ProfilingDataList { get; private set; }
    public INotifyCollectionChangedSynchronizedViewList<ProfilingDataItemViewModel> ProfilingDataListView { get; private set; }

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

        ProfilingDataList = [];
        ProfilingDataListView = ProfilingDataList.CreateView(x => x).ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
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
        if (Directory.Exists(path))
        {
            var files = await Task.Run(() => Directory.GetFiles(path, "*.json"));
            if (files.Length != 0)
            {
                var ps = await Task.WhenAll(
                    files.AsParallel().Select(async file =>
                    {
                        try
                        {
                            await using FileStream stream = File.OpenRead(file);
                            ProfilingData? data = await JsonSerializer.DeserializeAsync<ProfilingData>(stream, cancellationToken: ct);
                            return data == null
                                ? null
                                : new ProfilingDataItemViewModel()
                                {
                                    DataFilePath = file,
                                    Data = data
                                };
                        }
                        catch
                        {
                            return null;
                        }
                    })
                );
                var nps = ps.Compact().Where(x => !ProfilingDataList.Contains(x));
                if (nps.Any())
                {
                    ProfilingDataList.AddRange(nps.OrderByDescending(x => x.StartedAt));
                }
            }
        }
    }

    private async Task OnLogDirectoryPathChangedAsync(string path, CancellationToken ct)
    {
        ProfilingDataList.Clear();
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
                ProfilingDataList.Insert(0, new ProfilingDataItemViewModel()
                {
                    DataFilePath = e.FullPath,
                    Data = data
                });
            }
        }
        catch { }
    }

    private void OnProfileDataDeleted(object sender, FileSystemEventArgs e)
    {
        lock (ProfilingDataList.SyncRoot)
        {
            var item = ProfilingDataList.FirstOrDefault(x => x.DataFilePath == e.FullPath);
            if (item != null)
            {
                ProfilingDataList.Remove(item);
            }
        }
    }

    private void OnProfileDataRenamed(object sender, RenamedEventArgs e)
    {
        lock (ProfilingDataList.SyncRoot)
        {
            var item = ProfilingDataList.FirstOrDefault(x => x.DataFilePath == e.OldFullPath);
            if (item != null)
            {
                item.DataFilePath = e.FullPath;
            }
        }
    }

    [RelayCommand]
    public void Reload()
        => WebViewPile.RentSync(webView => webView.Refresh());
}
