using Avalonia.Data.Converters;
using Avalonia.Media;

using System.Net;

namespace Nonius.Converters;

public static class HttpStatusCodeConverters
{
    /// <summary>
    /// Converts an HttpStatusCode to a background color.
    /// </summary>
    public static readonly IValueConverter ToBackgroundColor =
        new FuncValueConverter<HttpStatusCode?, IBrush>(value =>
        {
            if (value is HttpStatusCode statusCode)
            {
                return (int)statusCode switch
                {
                    >= 200 and < 300 => SolidColorBrush.Parse("#41c464"), // Success
                    >= 300 and < 400 => SolidColorBrush.Parse("#f99d02"), // Redirection
                    >= 400 and < 500 => SolidColorBrush.Parse("#f99d02"), // Client error
                    >= 500 => SolidColorBrush.Parse("#eb364b"), // Server error
                    _ => Brushes.LightGray // Default for other codes
                };
            }
            return Brushes.Transparent;
        });
}
