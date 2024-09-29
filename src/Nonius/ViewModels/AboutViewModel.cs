using Avalonia.Platform;

using Nonius.Models;

namespace Nonius.ViewModels;

public partial class AboutViewModel : ViewModelBase
{
    public List<LicenseInfo> Licenses { get; set; } = [];

    public AboutViewModel()
    {
        using var s = AssetLoader.Open(new Uri("avares://Nonius/Assets/licenses.json"));
        var licenses = JsonSerializer.Deserialize<List<LicenseInfo>>(s) ?? [];

        // Overrides
        foreach (var license in licenses)
        {
            switch (license.PackageId)
            {
                case "HotAvalonia":
                case "HotAvalonia.Extensions":
                    license.License = "MIT";
                    license.LicenseUrl = "https://github.com/Kir-Antipov/HotAvalonia/blob/master/LICENSE.md";
                    break;
                case "RubyFlavor":
                    license.License = "MIT";
                    license.LicenseUrl = "https://opensource.org/licenses/MIT";
                    break;
                case "Sandreas.Avalonia.Preferences":
                    license.License = "Apache License 2.0";
                    license.LicenseUrl = "https://github.com/sandreas/Avalonia.Preferences/blob/main/LICENSE";
                    break;
                case "WebViewControl-Avalonia":
                case "WebViewControl-Avalonia-ARM64":
                    license.License = "Apache License 2.0";
                    license.LicenseUrl = "https://github.com/OutSystems/WebView/blob/master/LICENSE";
                    break;
                default:
                    break;
            }
            Licenses.Add(license);
        }
        Licenses.Add(new LicenseInfo()
        {
            PackageId = "いらすとや",
            PackageVersion = string.Empty,
            Authors = "いらすとや",
            License = string.Empty,
            LicenseUrl = "https://www.irasutoya.com/p/terms.html",
            Copyright = "Copyright © いらすとや. All Rights Reserved.",
            LicenseInformationOrigin = LicenseInformationOrigin.Expression
        });
    }
}
