using System.Threading;

using Nonius.Models.Vernier;

namespace Nonius.ViewModels;

internal static class ProfilingDataItemLoader
{
    public static async Task<IReadOnlyList<ProfilingDataItemViewModel>> LoadAsync(string path, CancellationToken ct = default)
    {
        if (!Directory.Exists(path))
        {
            return [];
        }

        string[] files = await Task.Run(() => Directory.GetFiles(path, "*.json"));
        if (files.Length == 0)
        {
            return [];
        }

        ProfilingDataItemViewModel?[] items = await Task.WhenAll(files.Select(file => LoadFileAsync(file, ct)));
        return items.
            Compact().
            OrderByDescending(x => x.StartedAt).
            ToArray();
    }

    private static async Task<ProfilingDataItemViewModel?> LoadFileAsync(string file, CancellationToken ct)
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
    }
}