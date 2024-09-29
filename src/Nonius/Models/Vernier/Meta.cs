using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;

/// <summary>
/// メタ情報を表すクラス
/// </summary>
public class Meta
{
    [JsonPropertyName("interval")]
    public required int Interval { get; init; }

    [JsonPropertyName("startTime")]
    public required double StartTime { get; init; }

    [JsonPropertyName("processType")]
    public required int ProcessType { get; init; }

    [JsonPropertyName("product")]
    public required string Product { get; init; }

    [JsonPropertyName("stackwalk")]
    public required int Stackwalk { get; init; }

    [JsonPropertyName("version")]
    public required int Version { get; init; }

    [JsonPropertyName("preprocessedProfileVersion")]
    public required int PreprocessedProfileVersion { get; init; }

    [JsonPropertyName("symbolicated")]
    public required bool Symbolicated { get; init; }

    [JsonPropertyName("markerSchema")]
    public required List<MarkerSchema> MarkerSchema { get; init; }

    [JsonPropertyName("sampleUnits")]
    public required SampleUnits SampleUnits { get; init; }

    [JsonPropertyName("categories")]
    public required List<Category> Categories { get; init; }

    [JsonPropertyName("sourceCodeIsNotOnSearchfox")]
    public required bool SourceCodeIsNotOnSearchfox { get; init; }
}
