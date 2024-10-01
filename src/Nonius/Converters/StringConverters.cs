using Avalonia.Data.Converters;

namespace Nonius.Converters;

/// <summary>
/// Provides a set of useful <see cref="IValueConverter"/>s for working with string values.
/// </summary>
public static class StringConverters
{
    /// <summary>
    /// A value converter that converts a string to uppercase.
    /// </summary>
    public static readonly IValueConverter Upper =
        new FuncValueConverter<string?, string?>(value => value?.ToUpper());
}
