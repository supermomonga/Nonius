using CommunityToolkit.Mvvm.Input;

using Epoxy;
using WebViewControl;
using Epoxy.Synchronized;
using Nonius.Models.Vernier;
using Avalonia.Controls;
using System.Threading;

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
        LogDirectoryPath.Subscribe(path => OnLogDirectoryPathChanged(path));

        Observable.Interval(TimeSpan.FromSeconds(1)).Select((_, i) => i).SubscribeAwait(async (x, ct) =>
        {
            await InvalidateProfilingDataListAsync(ct);
        });

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
                var url = $"https://vernier.prof/from-url/http%3A%2F%2Flocalhost%2F{fileName}";
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

    private async Task InvalidateProfilingDataListAsync(CancellationToken ct = default)
    {
        if (Directory.Exists(LogDirectoryPath.Value))
        {
            var files = await Task.Run(() => Directory.GetFiles(LogDirectoryPath.Value, "*.json"));
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

                    System.Console.WriteLine("Add pdl");
                    ProfilingDataList.AddRange(nps);
                    //ProfilingDataList.Sort();
                    //ProfilingDataList.Reverse();
                }
            }
        }
    }

    private void OnLogDirectoryPathChanged(string path)
    {
        ProfilingDataList.Clear();
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

        string jsonPath = Path.Join(LogDirectoryPath.Value, handler.Url.Split('/').Last());
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
