using Nonius.ViewModels;

using System.Collections.ObjectModel;

namespace Nonius.Tests;

public class ProfilingDataItemLoaderTests
{
    [Fact]
    public async Task LoadAsyncLoadsFixtureProfiles()
    {
        string sampleProfileDirectory = Path.Combine(AppContext.BaseDirectory, "Fixtures", "ProfileData");

        IReadOnlyList<ProfilingDataItemViewModel> items = await ProfilingDataItemLoader.LoadAsync(sampleProfileDirectory);

        Assert.Equal(2, items.Count);
        Assert.Equal(
            items.OrderByDescending(x => x.StartedAt).Select(x => x.DataFilePath),
            items.Select(x => x.DataFilePath));
        Assert.All(items, item =>
        {
            Assert.True(File.Exists(item.DataFilePath));
            Assert.NotEqual(default, item.ProfiledAt);
            Assert.Equal(16, item.DurationInMilliseconds);
        });
        Assert.Equal("ReportsController#index", items[0].ResourceName);
        Assert.Equal("GET", items[0].HttpMethod?.Method);
        Assert.Equal(System.Net.HttpStatusCode.OK, items[0].HttpStatusCode);
    }

    [Fact]
    public void ReadOnlyObservableCollectionCanBeCopiedForDataGridItemsSource()
    {
        ObservableCollection<string> source = [];
        ReadOnlyObservableCollection<string> view = new(source);

        source.Add("first");
        source.Add("second");

        Assert.Equal(["first", "second"], view.ToArray());
    }
}