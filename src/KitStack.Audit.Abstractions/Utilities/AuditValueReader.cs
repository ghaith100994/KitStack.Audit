using System.Globalization;

namespace KitStack.Audit.Abstractions.Utilities;

/// <summary>
/// Reads typed values back out of the loosely-typed dictionaries carried by
/// <c>TrailDto</c> and <c>AuditNarrationContext</c>. Values were normalized by
/// <see cref="AuditValueConverter"/>, so e.g. enums come back as their names and
/// dates as UTC <see cref="DateTime"/>s; this helper converts them to the requested type.
/// </summary>
public static class AuditValueReader
{
    public static T? Get<T>(IReadOnlyDictionary<string, object?> values, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (!values.TryGetValue(propertyName, out var value) || value is null)
            return default;

        return Convert(value, typeof(T)) is T typed ? typed : default;
    }

    private static object? Convert(object value, Type targetType)
    {
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlying.IsInstanceOfType(value))
            return value;

        try
        {
            if (underlying.IsEnum)
            {
                return value is string name
                    ? Enum.Parse(underlying, name, ignoreCase: true)
                    : Enum.ToObject(underlying, value);
            }

            if (underlying == typeof(Guid))
                return Guid.Parse(System.Convert.ToString(value, CultureInfo.InvariantCulture)!);

            if (underlying == typeof(DateOnly))
                return DateOnly.Parse(System.Convert.ToString(value, CultureInfo.InvariantCulture)!, CultureInfo.InvariantCulture);

            if (underlying == typeof(TimeOnly))
                return TimeOnly.Parse(System.Convert.ToString(value, CultureInfo.InvariantCulture)!, CultureInfo.InvariantCulture);

            return System.Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException or ArgumentException)
        {
            return null;
        }
    }
}
