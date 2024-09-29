using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;

/// <summary>
/// スタックテーブル情報
/// </summary>
public class StackTable
{
    [JsonPropertyName("frame")]
    public required List<int> Frame { get; init; }

    [JsonPropertyName("category")]
    public required List<int> Category { get; init; }

    [JsonPropertyName("subcategory")]
    public required List<int?> Subcategory { get; init; }
}
