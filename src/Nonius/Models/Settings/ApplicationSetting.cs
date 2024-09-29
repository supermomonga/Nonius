using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nonius.Models.Settings;

public class ApplicationSetting
{
    [JsonPropertyName("active_project_id")]
    public Guid? ActiveProjectId { get; set; }

    [JsonPropertyName("viewer_pane_width")]
    public double ViewerPaneWidth { get; set; }

    [JsonPropertyName("projects")]
    public required List<ProjectSetting> Projects { get; set; } = [];

    [JsonPropertyName("display_duration")]
    public TimeSpan? DisplayDuration { get; set; }
}
