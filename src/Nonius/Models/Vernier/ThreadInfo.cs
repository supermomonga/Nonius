using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;

/// <summary>
/// スレッド情報を表すクラス
/// </summary>
public class ThreadInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("isMainThread")]
    public required bool IsMainThread { get; init; }

    [JsonPropertyName("processStartupTime")]
    public required double ProcessStartupTime { get; init; }

    [JsonPropertyName("registerTime")]
    public required double RegisterTime { get; init; }

    [JsonPropertyName("pausedRanges")]
    public required List<object> PausedRanges { get; init; }

    [JsonPropertyName("pid")]
    public required int Pid { get; init; }

    [JsonPropertyName("tid")]
    public required int Tid { get; init; }

    [JsonPropertyName("frameTable")]
    public required FrameTable FrameTable { get; init; }

    [JsonPropertyName("funcTable")]
    public required FuncTable FuncTable { get; init; }

    [JsonPropertyName("nativeSymbols")]
    public required Dictionary<string, object> NativeSymbols { get; init; }

    [JsonPropertyName("samples")]
    public required Samples Samples { get; init; }

    [JsonPropertyName("markers")]
    public required Markers Markers { get; init; }

    [JsonPropertyName("stackTable")]
    public required StackTable StackTable { get; init; }
}
