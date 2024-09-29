using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;

/// <summary>
/// サンプル情報
/// </summary>
public class Samples
{
    [JsonPropertyName("stack")]
    public required List<int> Stack { get; init; }

    [JsonPropertyName("time")]
    public required List<double> Time { get; init; }

    [JsonPropertyName("weight")]
    public required List<int> Weight { get; init; }

    [JsonPropertyName("weightType")]
    public required string WeightType { get; init; }

    [JsonPropertyName("length")]
    public required int Length { get; init; }
}
