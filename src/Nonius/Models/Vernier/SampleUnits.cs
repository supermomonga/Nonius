using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;

/// <summary>
/// サンプルの単位情報
/// </summary>
public class SampleUnits
{
    [JsonPropertyName("time")]
    public required string Time { get; init; }

    [JsonPropertyName("eventDelay")]
    public required string EventDelay { get; init; }

    [JsonPropertyName("threadCPUDelta")]
    public required string ThreadCPUDelta { get; init; }
}
