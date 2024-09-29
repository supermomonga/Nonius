using System.Text.Json.Serialization;

namespace Nonius.Models;
public class LicenseInfo
{
    [JsonPropertyName("PackageId")]
    public required string PackageId { get; set; }

    [JsonPropertyName("PackageVersion")]
    public required string PackageVersion { get; set; }

    [JsonPropertyName("PackageProjectUrl")]
    public string? PackageProjectUrl { get; set; }

    [JsonPropertyName("Copyright")]
    public string? Copyright { get; set; }

    [JsonPropertyName("Authors")]
    public required string Authors { get; set; }

    [JsonPropertyName("ValidationErrors")]
    public List<ValidationError> ValidationErrors { get; set; } = [];

    [JsonPropertyName("License")]
    public string? License { get; set; }

    [JsonPropertyName("LicenseUrl")]
    public string? LicenseUrl { get; set; }

    [JsonPropertyName("LicenseInformationOrigin")]
    public required LicenseInformationOrigin LicenseInformationOrigin { get; set; }
}

public class ValidationError
{
    [JsonPropertyName("Error")]
    public required string Error { get; set; }

    [JsonPropertyName("Context")]
    public required string Context { get; set; }
}

public enum LicenseInformationOrigin
{
    Expression = 0,
    Url = 1,
    Unknown = 2,
    Ignored = 3,
    Overwrite = 4
}
