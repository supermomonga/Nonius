﻿using CommunityToolkit.Mvvm.Input;

using Epoxy;
using WebViewControl;
using Epoxy.Synchronized;
using Nonius.Models.Vernier;
using Avalonia.Controls;
using System.Threading;
using System.Web;

namespace Nonius.ViewModels;

public partial class MonitoringViewModel : ViewModelBase
{
    private readonly ApplicationContextViewModel _Context;
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
    public BindableReactiveProperty<string> BrowserAddress { get; } = new();

    public Pile<WebView> WebViewPile { get; } = Pile.Factory.Create<WebView>();
    public Well<WebView> WebViewWell { get; } = Well.Factory.Create<WebView>();
    public Well<SplitView> SplitViewWell { get; } = Well.Factory.Create<SplitView>();
    public Well<UserControl> ContainerWell { get; } = Well.Factory.Create<UserControl>();

    public Uri NoniusUri { get; } = new("https://vernier.prof/");


    private FileSystemWatcher? _Watcher;
    public MonitoringViewModel(ApplicationContextViewModel context)
    {
        System.Console.WriteLine($"Thread: {Environment.CurrentManagedThreadId}");
        _Context = context;
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
                var fileName = Path.GetFileName(x.DataFilePath);
                var fileUrl = HttpUtility.UrlEncode($"http://localhost/{fileName}");
                var url = $"https://vernier.prof/from-url/{fileUrl}";
                if (BrowserAddress.Value != url)
                {
                    BrowserAddress.Value = url;
                }
            }
        });

        // TODO: NativeAOT では文字列ベースのイベント名を認識できないので、Routed Events の実装が必要
        // WebViewWell.Add<ResourceHandler>("BeforeResourceLoad", ResourceLoadHandler);
        WebViewWell.Add(Control.LoadedEvent, () =>
        {
            WebViewPile.RentSync(webView => webView.BeforeResourceLoad += BeforeResourceLoadHandler);
            return ValueTask.CompletedTask;
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
                            var data = await JsonSerializer.DeserializeAsync<ProfilingData>(File.OpenRead(file));
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
            var data = JsonSerializer.Deserialize<ProfilingData>(File.OpenRead(e.FullPath));
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

    public void BeforeResourceLoadHandler(ResourceHandler handler)
    {
        if (handler.Handled)
            return;

        if (!handler.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
            return;

        if (!handler.Url.StartsWith("http://localhost/", StringComparison.InvariantCultureIgnoreCase) ||
            !handler.Url.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
            return;

        string jsonPath = Path.Join(LogDirectoryPath.Value, HttpUtility.UrlDecode(handler.Url.Split('/').Last()));
        if (!File.Exists(jsonPath))
            return;

        handler.RespondWith(jsonPath);
    }

    [RelayCommand]
    public void Reload()
        => WebViewPile.RentSync(webView => webView.Reload());

    [RelayCommand]
    public void OpenDeveloperTools()
        => WebViewPile.RentSync(webView => webView.ShowDeveloperTools());
}
