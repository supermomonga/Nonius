using Avalonia.Data.Converters;

namespace Nonius.Converters;

/// <summary>
/// Provides a set of useful <see cref="IValueConverter"/>s for working with enum values.
/// </summary>
public static class EnumConverters
{
    /// <summary>
    /// A value converter that converts an Enum to its integer value.
    /// </summary>
    public static readonly IValueConverter IntValue =
        new FuncValueConverter<Enum, int>(value => Convert.ToInt32(value));
}
