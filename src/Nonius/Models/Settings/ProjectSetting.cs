using System.Text.Json.Serialization;

namespace Nonius.Models.Settings;

public class ProjectSetting
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("profile_data_dir")]
    public required string ProfileDataDir { get; set; }

    [JsonPropertyName("source_code_dir")]
    public string? SourceCodeDir { get; set; }
}
