using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;

/// <summary>
/// マーカーのスキーマ定義
/// </summary>
public class MarkerSchema
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("display")]
    public required List<string> Display { get; init; }

    [JsonPropertyName("tooltipLabel")]
    public string? TooltipLabel { get; init; }

    [JsonPropertyName("chartLabel")]
    public string? ChartLabel { get; init; }

    [JsonPropertyName("tableLabel")]
    public string? TableLabel { get; init; }

    [JsonPropertyName("data")]
    public required List<MarkerData> Data { get; init; }
}
