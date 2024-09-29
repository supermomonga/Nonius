using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nonius.Models.Vernier;

/// <summary>
/// カテゴリー情報
/// </summary>
public class Category
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("color")]
    public required string Color { get; init; }

    [JsonPropertyName("subcategories")]
    public required List<string> Subcategories { get; init; }
}
