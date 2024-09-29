using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;

/// <summary>
/// マーカーのデータフィールド
/// </summary>
public class MarkerData
{
    [JsonPropertyName("label")]
    public string? Label { get; init; }

    [JsonPropertyName("value")]
    public string? Value { get; init; }

    [JsonPropertyName("key")]
    public string? Key { get; init; }

    [JsonPropertyName("format")]
    public string? Format { get; init; }

    [JsonPropertyName("searchable")]
    public bool? Searchable { get; init; }
}
