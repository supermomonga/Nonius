using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;

/// <summary>
/// Markers情報
/// </summary>
public class Markers
{
    [JsonPropertyName("data")]
    public required List<MarkerDataItem> Data { get; init; }

    [JsonPropertyName("name")]
    public required List<int> Name { get; init; }

    [JsonPropertyName("startTime")]
    public required List<double?> StartTime { get; init; }

    [JsonPropertyName("endTime")]
    public required List<double?> EndTime { get; init; }

    [JsonPropertyName("phase")]
    public required List<int> Phase { get; init; }

    [JsonPropertyName("category")]
    public required List<int> Category { get; init; }

    [JsonPropertyName("length")]
    public required int Length { get; init; }
}
