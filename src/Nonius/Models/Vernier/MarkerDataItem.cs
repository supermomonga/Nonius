using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;

public class MarkerDataItem
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("controller")]
    public string? Controller { get; init; }

    [JsonPropertyName("action")]
    public string? Action { get; init; }

    [JsonPropertyName("status")]
    public int? Status { get; init; }

    [JsonPropertyName("path")]
    public string? Path { get; init; }

    [JsonPropertyName("method")]
    public string? Method { get; init; }

    [JsonPropertyName("format")]
    public string? Format { get; init; }
}
