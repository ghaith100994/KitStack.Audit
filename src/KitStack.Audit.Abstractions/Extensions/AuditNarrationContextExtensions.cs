using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Abstractions.Utilities;

namespace KitStack.Audit.Abstractions.Extensions;

/// <summary>
/// Typed accessors for the value snapshots carried by an <see cref="AuditNarrationContext"/>,
/// so narrators can read values without manual dictionary plumbing.
/// </summary>
public static class AuditNarrationContextExtensions
{
    /// <summary>Whether <paramref name="propertyName"/> changed in this trail (Update trails).</summary>
    public static bool HasChanged(this AuditNarrationContext context, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.ModifiedProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>The pre-change value of <paramref name="propertyName"/>, converted to <typeparamref name="T"/>.</summary>
    public static T? GetOldValue<T>(this AuditNarrationContext context, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(context);
        return AuditValueReader.Get<T>(context.OldValues, propertyName);
    }

    /// <summary>The post-change value of <paramref name="propertyName"/>, converted to <typeparamref name="T"/>.</summary>
    public static T? GetNewValue<T>(this AuditNarrationContext context, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(context);
        return AuditValueReader.Get<T>(context.NewValues, propertyName);
    }

    /// <summary>
    /// The current value of <paramref name="propertyName"/>: the new value when present,
    /// otherwise the old value (e.g. for Delete trails).
    /// </summary>
    public static T? GetValue<T>(this AuditNarrationContext context, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.NewValues.ContainsKey(propertyName)
            ? AuditValueReader.Get<T>(context.NewValues, propertyName)
            : AuditValueReader.Get<T>(context.OldValues, propertyName);
    }
}
