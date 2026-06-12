using System.Globalization;

namespace KitStack.Audit.Abstractions.Utilities;

/// <summary>
/// Normalizes property values so every sink serializes them consistently.
/// Dates are coerced to UTC; DateOnly/TimeOnly are formatted to invariant strings;
/// enums are stored by name and binary blobs as Base64.
/// </summary>
public static class AuditValueConverter
{
    public static object? Normalize(object? value, CultureInfo? culture = null)
    {
        if (value is null)
            return null;

        culture ??= CultureInfo.InvariantCulture;

        return value switch
        {
            DateOnly dateOnly => dateOnly.ToString("yyyy-MM-dd", culture),
            TimeOnly timeOnly => timeOnly.ToString("HH:mm:ss.fffffff", culture),
            DateTime dateTime => dateTime.Kind == DateTimeKind.Unspecified
                ? new DateTime(dateTime.Ticks, DateTimeKind.Utc)
                : dateTime.ToUniversalTime(),
            DateTimeOffset dateTimeOffset => dateTimeOffset.UtcDateTime,
            Enum enumValue => enumValue.ToString(),
            byte[] bytes => Convert.ToBase64String(bytes),
            string str when DateTime.TryParse(str, culture, DateTimeStyles.None, out var dt)
                => dt.ToUniversalTime(),
            _ => value
        };
    }
}
