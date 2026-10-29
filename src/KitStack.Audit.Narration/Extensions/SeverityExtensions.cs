using KitStack.Audit.Narration.Enums;

namespace KitStack.Audit.Narration.Extensions;

public static class SeverityExtensions
{
    /// <summary>The string persisted on <c>ActivityEvent.Severity</c> ("info", "success", "warning", "critical").</summary>
    public static string ToWireValue(this Severity severity) => severity switch
    {
        Severity.Info => "info",
        Severity.Success => "success",
        Severity.Warning => "warning",
        Severity.Critical => "critical",
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null),
    };

    /// <summary>Parses a persisted severity string back into a <see cref="Severity"/>; null when unknown.</summary>
    public static Severity? ParseSeverity(string? value) => value?.ToUpperInvariant() switch
    {
        "INFO" => Severity.Info,
        "SUCCESS" => Severity.Success,
        "WARNING" => Severity.Warning,
        "CRITICAL" => Severity.Critical,
        _ => null,
    };
}
