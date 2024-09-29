using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;

/// <summary>
/// 関数テーブル情報
/// </summary>
public class FuncTable
{
    [JsonPropertyName("name")]
    public required List<int> Name { get; init; }

    [JsonPropertyName("isJS")]
    public required List<bool> IsJs { get; init; }

    [JsonPropertyName("relevantForJS")]
    public required List<bool> RelevantForJs { get; init; }

    [JsonPropertyName("resource")]
    public required List<int> Resource { get; init; }

    [JsonPropertyName("fileName")]
    public required List<int> FileName { get; init; }

    [JsonPropertyName("lineNumber")]
    public required List<int?> LineNumber { get; init; }

    [JsonPropertyName("columnNumber")]
    public required List<int?> ColumnNumber { get; init; }

    [JsonPropertyName("length")]
    public required int Length { get; init; }
}
