using System.Collections.Concurrent;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Avalonia.Platform;

namespace Nonius.Services;

public sealed class ProfileViewerServer : IDisposable
{
    private const string ProfileViewerArchiveUri = "avares://Nonius/Assets/ProfileViewer/profile-viewer-0.0.7.zip";

    private static readonly Lazy<IReadOnlyDictionary<string, byte[]>> s_assets = new(LoadAssets);

    private readonly ConcurrentDictionary<string, string> _profiles = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly TcpListener _listener;
    private readonly Task _serverTask;

    public Uri BaseUri { get; }

    public ProfileViewerServer()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();

        int port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        BaseUri = new Uri($"http://127.0.0.1:{port}/");
        _serverTask = Task.Run(() => RunAsync(_cancellationTokenSource.Token));
    }

    public Uri GetProfileViewerUri(string profilePath)
    {
        string fullPath = Path.GetFullPath(profilePath);
        string profileId = GetProfileId(fullPath);
        _profiles[profileId] = fullPath;

        Uri profileUri = new(BaseUri, $"profiles/{profileId}.json");
        return new Uri(BaseUri, $"from-url/{Uri.EscapeDataString(profileUri.AbsoluteUri)}");
    }

    private async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            _ = Task.Run(() => ProcessClientAsync(client, ct), ct);
        }
    }

    private async Task ProcessClientAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        {
            await using NetworkStream stream = client.GetStream();
            using StreamReader reader = new(stream, Encoding.ASCII, leaveOpen: true);

            string? requestLine = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(requestLine))
            {
                return;
            }

            string[] requestParts = requestLine.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (requestParts.Length < 2)
            {
                await WriteTextResponseAsync(stream, HttpStatusCode.BadRequest, "Bad Request", ct);
                return;
            }

            string method = requestParts[0];
            bool headOnly = method.Equals("HEAD", StringComparison.OrdinalIgnoreCase);
            if (!headOnly && !method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                await WriteTextResponseAsync(stream, HttpStatusCode.MethodNotAllowed, "Method Not Allowed", ct);
                return;
            }

            while (await reader.ReadLineAsync(ct) is { Length: > 0 })
            {
            }

            try
            {
                await HandleRequestAsync(stream, requestParts[1], headOnly, ct);
            }
            catch
            {
                await WriteTextResponseAsync(stream, HttpStatusCode.InternalServerError, "Internal Server Error", ct);
            }
        }
    }

    private async Task HandleRequestAsync(Stream stream, string requestTarget, bool headOnly, CancellationToken ct)
    {
        string path = GetPath(requestTarget);

        if (path.StartsWith("/profiles/", StringComparison.OrdinalIgnoreCase))
        {
            await ServeProfileAsync(stream, path, headOnly, ct);
            return;
        }

        string assetPath = path is "/" or "/from-url" || path.StartsWith("/from-url/", StringComparison.OrdinalIgnoreCase)
            ? "index.html"
            : NormalizeAssetPath(path);

        if (assetPath.Length == 0 || !s_assets.Value.TryGetValue(assetPath, out byte[]? asset))
        {
            await WriteTextResponseAsync(stream, HttpStatusCode.NotFound, "Not Found", ct);
            return;
        }

        await WriteResponseAsync(stream, HttpStatusCode.OK, GetContentType(assetPath), asset, headOnly, ct);
    }

    private async Task ServeProfileAsync(Stream stream, string path, bool headOnly, CancellationToken ct)
    {
        string profileId = Path.GetFileNameWithoutExtension(path);
        if (!_profiles.TryGetValue(profileId, out string? profilePath) || !File.Exists(profilePath))
        {
            await WriteTextResponseAsync(stream, HttpStatusCode.NotFound, "Not Found", ct);
            return;
        }

        byte[] body = await File.ReadAllBytesAsync(profilePath, ct);
        await WriteResponseAsync(stream, HttpStatusCode.OK, "application/json", body, headOnly, ct);
    }

    private static string GetPath(string requestTarget)
    {
        int queryIndex = requestTarget.IndexOf('?');
        string path = queryIndex >= 0 ? requestTarget[..queryIndex] : requestTarget;
        return Uri.UnescapeDataString(path);
    }

    private static string NormalizeAssetPath(string path)
    {
        string assetPath = path.TrimStart('/').Replace('\\', '/');
        if (assetPath.Split('/').Any(static part => part is ".." or "."))
        {
            return string.Empty;
        }

        return assetPath;
    }

    private static async Task WriteTextResponseAsync(
        Stream stream,
        HttpStatusCode statusCode,
        string message,
        CancellationToken ct)
    {
        byte[] body = Encoding.UTF8.GetBytes(message);
        await WriteResponseAsync(stream, statusCode, "text/plain; charset=utf-8", body, false, ct);
    }

    private static async Task WriteResponseAsync(
        Stream stream,
        HttpStatusCode statusCode,
        string contentType,
        byte[] body,
        bool headOnly,
        CancellationToken ct)
    {
        string headers =
            $"HTTP/1.1 {(int)statusCode} {GetReasonPhrase(statusCode)}\r\n" +
            $"Content-Type: {contentType}\r\n" +
            $"Content-Length: {body.Length}\r\n" +
            "Cache-Control: no-store\r\n" +
            "Access-Control-Allow-Origin: *\r\n" +
            "Connection: close\r\n" +
            "\r\n";

        await stream.WriteAsync(Encoding.ASCII.GetBytes(headers), ct);
        if (!headOnly)
        {
            await stream.WriteAsync(body, ct);
        }
    }

    private static string GetReasonPhrase(HttpStatusCode statusCode) =>
        statusCode switch
        {
            HttpStatusCode.OK => "OK",
            HttpStatusCode.BadRequest => "Bad Request",
            HttpStatusCode.InternalServerError => "Internal Server Error",
            HttpStatusCode.NotFound => "Not Found",
            HttpStatusCode.MethodNotAllowed => "Method Not Allowed",
            _ => statusCode.ToString()
        };

    private static string GetContentType(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".css" => "text/css; charset=utf-8",
            ".ftl" => "text/plain; charset=utf-8",
            ".html" => "text/html; charset=utf-8",
            ".ico" => "image/x-icon",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".js" => "text/javascript; charset=utf-8",
            ".json" or ".map" => "application/json",
            ".md" => "text/markdown; charset=utf-8",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            ".txt" => "text/plain; charset=utf-8",
            ".wasm" => "application/wasm",
            _ => "application/octet-stream"
        };

    private static string GetProfileId(string path)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(path));
        return Convert.ToHexString(hash)[..32].ToLowerInvariant();
    }

    private static IReadOnlyDictionary<string, byte[]> LoadAssets()
    {
        Dictionary<string, byte[]> assets = new(StringComparer.OrdinalIgnoreCase);

        using Stream archiveStream = AssetLoader.Open(new Uri(ProfileViewerArchiveUri));
        using ZipArchive archive = new(archiveStream, ZipArchiveMode.Read);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
            {
                continue;
            }

            using Stream entryStream = entry.Open();
            using MemoryStream memoryStream = new();
            entryStream.CopyTo(memoryStream);
            assets[entry.FullName.Replace('\\', '/')] = memoryStream.ToArray();
        }

        return assets;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();

        try
        {
            _serverTask.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
        }

        _cancellationTokenSource.Dispose();
    }
}
