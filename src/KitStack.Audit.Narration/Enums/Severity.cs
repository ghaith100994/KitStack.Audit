namespace KitStack.Audit.Narration.Enums;

/// <summary>
/// Visual weight of an activity event. Stored on <c>ActivityEvent.Severity</c> as its
/// lowercase name (see <c>SeverityExtensions.ToWireValue</c>), so existing rows stay readable.
/// </summary>
public enum Severity
{
    Info,
    Success,
    Warning,
    Critical,
}
