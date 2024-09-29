using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;

/// <summary>
/// フレームテーブル情報
/// </summary>
public class FrameTable
{
    [JsonPropertyName("address")]
    public required List<int> Address { get; init; }

    [JsonPropertyName("inlineDepth")]
    public required List<int> InlineDepth { get; init; }

    [JsonPropertyName("category")]
    public required List<int> Category { get; init; }

    [JsonPropertyName("subcategory")]
    public required List<int?> Subcategory { get; init; }

    [JsonPropertyName("func")]
    public required List<int> Func { get; init; }

    [JsonPropertyName("nativeSymbol")]
    public required List<object> NativeSymbol { get; init; }

    [JsonPropertyName("innerWindowID")]
    public required List<object> InnerWindowID { get; init; }

    [JsonPropertyName("implementation")]
    public required List<int?> Implementation { get; init; }

    [JsonPropertyName("line")]
    public required List<int?> Line { get; init; }

    [JsonPropertyName("column")]
    public required List<object> Column { get; init; }

    [JsonPropertyName("length")]
    public required int Length { get; init; }
}
