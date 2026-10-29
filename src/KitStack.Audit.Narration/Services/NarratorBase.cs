using System.Globalization;
using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Extensions;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Abstractions.Services;
using KitStack.Audit.Narration.Contracts;
using KitStack.Audit.Narration.Enums;
using KitStack.Audit.Narration.Extensions;
using KitStack.Audit.Narration.Models;
using KitStack.Audit.Narration.Options;

namespace KitStack.Audit.Narration.Services;

/// <summary>
/// Base for narrators that turn raw audit trails into bilingual activity events.
/// Soft delete (soft-delete property set) and restore (cleared) arrive as updates and are narrated as such.
/// </summary>
public abstract class NarratorBase<TEntity> : AuditNarratorBase<TEntity>
    where TEntity : class
{
    protected NarratorBase(INarrationServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Services = services;
    }

    protected INarrationServices Services { get; }

    protected NarrationOptions Options => Services.Options;

    protected INarrationPhrases Phrases => Services.Phrases;

    /// <summary>Properties tried in order by <see cref="Name"/>. Override to give a module its own list.</summary>
    protected virtual IEnumerable<string> NameProperties => Options.NameProperties;

    // ─── Core Value Helpers ─────────────────────────────

    /// <summary>New value when present, otherwise the old one (deletes only carry old values).</summary>
    protected static string? Val(AuditNarrationContext c, string property)
    {
        ArgumentNullException.ThrowIfNull(c);
        if (c.NewValues.TryGetValue(property, out var n) && n is not null) return Text(n);
        if (c.OldValues.TryGetValue(property, out var o) && o is not null) return Text(o);
        return null;
    }

    /// <summary>Typed value: the new value when present, otherwise the old one, converted to <typeparamref name="T"/>.</summary>
    protected static T? Val<T>(AuditNarrationContext c, string property)
    {
        ArgumentNullException.ThrowIfNull(c);
        return c.NewValues.TryGetValue(property, out var n) && n is not null
            ? c.GetNewValue<T>(property)
            : c.GetOldValue<T>(property);
    }

    /// <summary>First non-null value among several property names.</summary>
    protected static string? ValAny(AuditNarrationContext c, params string[] properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        return properties.Select(p => Val(c, p)).FirstOrDefault(v => v is not null);
    }

    protected static string? OldVal(AuditNarrationContext c, string property)
    {
        ArgumentNullException.ThrowIfNull(c);
        return c.OldValues.TryGetValue(property, out var o) && o is not null ? Text(o) : null;
    }

    protected static T? OldVal<T>(AuditNarrationContext c, string property)
    {
        ArgumentNullException.ThrowIfNull(c);
        return c.GetOldValue<T>(property);
    }

    protected static T? NewVal<T>(AuditNarrationContext c, string property)
    {
        ArgumentNullException.ThrowIfNull(c);
        return c.GetNewValue<T>(property);
    }

    protected static bool? BoolVal(AuditNarrationContext c, string property)
        => NewRaw(c, property) is bool b ? b : null;

    protected static int? IntVal(AuditNarrationContext c, string property)
        => NewRaw(c, property) is int i ? i : null;

    protected static long? LongVal(AuditNarrationContext c, string property)
        => NewRaw(c, property) is long l ? l : null;

    protected static DateTime? DateTimeVal(AuditNarrationContext c, string property)
        => NewRaw(c, property) is DateTime dt ? dt : null;

    protected static Guid? GuidVal(AuditNarrationContext c, string property)
        => NewRaw(c, property) is Guid g ? g : null;

    // ─── Enum labels ────────────────────────────────────

    /// <summary>Label of an enum value read from the trail (via <see cref="IEnumLabelProvider"/>), falling back to its raw text.</summary>
    protected LocalizedText EnumLabel(object? value) => value switch
    {
        null => LocalizedText.Same(Phrases.EmptyValue),
        Enum e => Services.EnumLabels.GetLabel(e),
        _ => LocalizedText.Same(Text(value) ?? Phrases.EmptyValue),
    };

    /// <summary>Label of a trail value of enum <typeparamref name="TEnum"/>, whether the trail stored the enum, its name or its number.</summary>
    protected LocalizedText EnumLabel<TEnum>(object? value) where TEnum : struct, Enum
        => ToEnum<TEnum>(value) is { } e ? EnumLabel(e) : EnumLabel(value);

    protected static TEnum? ToEnum<TEnum>(object? value) where TEnum : struct, Enum => value switch
    {
        null => null,
        TEnum e => e,
        Enum other => Enum.TryParse<TEnum>(other.ToString(), out var byName) ? byName : null,
        _ when Enum.TryParse<TEnum>(Text(value), true, out var parsed) => parsed,
        _ => null,
    };

    protected LocalizedText NewEnum<TEnum>(AuditNarrationContext c, string property) where TEnum : struct, Enum
        => EnumLabel<TEnum>(NewRaw(c, property));

    protected LocalizedText OldEnum<TEnum>(AuditNarrationContext c, string property) where TEnum : struct, Enum
        => EnumLabel<TEnum>(OldRaw(c, property));

    protected static TEnum? NewEnumValue<TEnum>(AuditNarrationContext c, string property) where TEnum : struct, Enum
        => ToEnum<TEnum>(NewRaw(c, property));

    protected static TEnum? OldEnumValue<TEnum>(AuditNarrationContext c, string property) where TEnum : struct, Enum
        => ToEnum<TEnum>(OldRaw(c, property));

    /// <summary>Update that set <paramref name="property"/> to <paramref name="value"/>.</summary>
    protected static bool Became<TEnum>(AuditNarrationContext c, string property, TEnum value) where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(c);
        return c.TrailType == TrailType.Update
               && c.ModifiedProperties.Contains(property)
               && NewEnumValue<TEnum>(c, property) is { } v && v.Equals(value);
    }

    /// <summary>Update moving <paramref name="property"/> from <paramref name="from"/> to <paramref name="to"/>.</summary>
    protected static bool Moved<TEnum>(AuditNarrationContext c, string property, TEnum from, TEnum to) where TEnum : struct, Enum
        => Became(c, property, to) && OldEnumValue<TEnum>(c, property) is { } o && o.Equals(from);

    /// <summary>Update whose old value of <paramref name="property"/> was one of <paramref name="values"/>.</summary>
    protected static bool WasIn<TEnum>(AuditNarrationContext c, string property, params TEnum[] values) where TEnum : struct, Enum
        => OldEnumValue<TEnum>(c, property) is { } o && values.Contains(o);

    // ─── Name Resolution ────────────────────────────────

    /// <summary>Display name of the entity: the first non-null of <see cref="NameProperties"/>, else the primary key.</summary>
    protected virtual string Name(AuditNarrationContext c)
    {
        ArgumentNullException.ThrowIfNull(c);
        foreach (var property in NameProperties)
        {
            if (Val(c, property) is { } value)
                return value;
        }

        return c.PrimaryKey ?? string.Empty;
    }

    // ─── Change Detection ───────────────────────────────

    protected static bool Modified(AuditNarrationContext c, params string[] properties)
    {
        ArgumentNullException.ThrowIfNull(c);
        ArgumentNullException.ThrowIfNull(properties);
        return properties.Any(p => c.ModifiedProperties.Contains(p));
    }

    /// <summary>Modified properties minus <see cref="NarrationOptions.SystemFields"/>.</summary>
    protected IReadOnlyList<string> UserFields(AuditNarrationContext c)
    {
        ArgumentNullException.ThrowIfNull(c);
        return c.ModifiedProperties.Where(p => !Options.SystemFields.Contains(p)).ToList();
    }

    protected static bool Changed(AuditNarrationContext c, string property)
    {
        ArgumentNullException.ThrowIfNull(c);
        return c.TrailType == TrailType.Create
            ? c.NewValues.TryGetValue(property, out var v) && v is not null
            : c.ModifiedProperties.Contains(property);
    }

    /// <summary>Soft delete: an update that set the soft-delete property.</summary>
    protected bool SoftDeleted(AuditNarrationContext c)
    {
        ArgumentNullException.ThrowIfNull(c);
        var property = Options.SoftDeleteProperty;
        return c.TrailType == TrailType.Update && c.ModifiedProperties.Contains(property)
               && c.NewValues.TryGetValue(property, out var v) && v is not null;
    }

    /// <summary>Restore: an update that cleared the soft-delete property.</summary>
    protected bool Restored(AuditNarrationContext c)
    {
        ArgumentNullException.ThrowIfNull(c);
        var property = Options.SoftDeleteProperty;
        return c.TrailType == TrailType.Update && c.ModifiedProperties.Contains(property)
               && (!c.NewValues.TryGetValue(property, out var v) || v is null);
    }

    /// <summary>Hard or soft delete.</summary>
    protected bool Removed(AuditNarrationContext c)
    {
        ArgumentNullException.ThrowIfNull(c);
        return c.TrailType == TrailType.Delete || SoftDeleted(c);
    }

    // ─── Formatting ─────────────────────────────────────

    /// <summary>A date from the trail, whether it was stored as DateTime or text, as yyyy-MM-dd (HH:mm when it has a time).</summary>
    protected static string? When(object? value) => value switch
    {
        null => null,
        DateTime dt => dt.ToString(dt.TimeOfDay == TimeSpan.Zero ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        _ when DateTime.TryParse(Text(value), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) => When(parsed),
        _ => Text(value),
    };

    protected static string? NewWhen(AuditNarrationContext c, string property) => When(NewRaw(c, property));

    protected static string? OldWhen(AuditNarrationContext c, string property) => When(OldRaw(c, property));

    /// <summary>The non-blank parts joined with " — ", or null when all are blank.</summary>
    protected static string? Join(params string?[] parts)
    {
        ArgumentNullException.ThrowIfNull(parts);
        var present = parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        return present.Count > 0 ? string.Join(" — ", present) : null;
    }

    /// <summary>The first <paramref name="max"/> characters of a long text, for subtitles.</summary>
    protected static string? Excerpt(string? text, int max = 120)
        => text is null ? null : text.Length <= max ? text : string.Concat(text.AsSpan(0, max).TrimEnd(), "…");

    // ─── Metadata Builders ──────────────────────────────

    /// <summary>A metadata dictionary of the non-null pairs, or null when none are set.</summary>
    protected static IDictionary<string, string>? Meta(params (string key, string? value)[] pairs)
    {
        ArgumentNullException.ThrowIfNull(pairs);
        var dict = pairs.Where(p => p.value is not null)
                        .ToDictionary(p => p.key, p => p.value!);
        return dict.Count == 0 ? null : dict;
    }

    protected static IDictionary<string, string>? ChangedFrom(AuditNarrationContext c, string property)
        => Meta(("property", property), ("from", OldVal(c, property)), ("to", Val(c, property)));

    // ─── Event Builder ──────────────────────────────────

    protected ActivityEvent Event(
        AuditNarrationContext c,
        string icon,
        Severity severity,
        LocalizedText title,
        LocalizedText? subtitle = null,
        IDictionary<string, string>? meta = null)
    {
        ArgumentNullException.ThrowIfNull(c);
        return new ActivityEvent
        {
            EntityType = typeof(TEntity).Name,
            EntityId = c.PrimaryKey,
            Module = c.Module,
            Action = c.TrailType,
            UserId = c.UserId,
            Timestamp = c.Timestamp,
            Icon = icon,
            Severity = severity.ToWireValue(),
            TitleEn = title.En,
            TitleAr = title.Ar,
            SubtitleEn = subtitle?.En,
            SubtitleAr = subtitle?.Ar,
            Metadata = meta,
        };
    }

    /// <summary>"{label} changed: name" with "old → new" for an enum property.</summary>
    protected ActivityEvent EnumChange<TEnum>(AuditNarrationContext c, string property, string icon, Severity severity, LocalizedText label)
        where TEnum : struct, Enum
        => Event(c, icon, severity,
            Phrases.Changed(label, Name(c)),
            Phrases.Transition(OldEnum<TEnum>(c, property), NewEnum<TEnum>(c, property)),
            ChangedFrom(c, property));

    // ─── Standard Patterns ──────────────────────────────

    /// <summary>
    /// Created / updated (user fields only) / removed (hard or soft) / restored.
    /// Returns null for an update that touched only system fields.
    /// </summary>
    protected ActivityEvent? Standard(
        AuditNarrationContext c,
        string icon,
        LocalizedText label,
        string? createIcon = null,
        string? deleteIcon = null)
    {
        ArgumentNullException.ThrowIfNull(c);
        var name = Name(c);

        if (Removed(c))
            return Event(c, deleteIcon ?? Options.Icons.Removed, Severity.Critical, Phrases.Removed(label, name));

        if (Restored(c))
            return Event(c, Options.Icons.Restored, Severity.Success, Phrases.Restored(label, name));

        var userFields = UserFields(c);
        return c.TrailType switch
        {
            TrailType.Create => Event(c, createIcon ?? icon, Severity.Success, Phrases.Created(label, name)),

            TrailType.Update when userFields.Count == 0 => null,

            TrailType.Update => Event(c, icon, Severity.Info,
                Phrases.Updated(label, name),
                Phrases.FieldsChanged(userFields.Count),
                new Dictionary<string, string> { ["fields"] = string.Join(", ", userFields) }),

            _ => null,
        };
    }

    /// <summary><see cref="Standard"/> plus activated/deactivated when the activation property changed.</summary>
    protected ActivityEvent? StandardWithActivation(AuditNarrationContext c, string icon, LocalizedText label)
    {
        ArgumentNullException.ThrowIfNull(c);
        var property = Options.ActivationProperty;
        if (c.TrailType == TrailType.Update && c.ModifiedProperties.Contains(property))
        {
            var name = Name(c);
            return BoolVal(c, property) == true
                ? Event(c, Options.Icons.Activated, Severity.Success, Phrases.Activated(label, name), Phrases.StatusActive)
                : Event(c, Options.Icons.Deactivated, Severity.Warning, Phrases.Deactivated(label, name), Phrases.StatusInactive);
        }

        return Standard(c, icon, label);
    }

    private static object? NewRaw(AuditNarrationContext c, string property)
    {
        ArgumentNullException.ThrowIfNull(c);
        return c.NewValues.TryGetValue(property, out var v) ? v : null;
    }

    private static object? OldRaw(AuditNarrationContext c, string property)
    {
        ArgumentNullException.ThrowIfNull(c);
        return c.OldValues.TryGetValue(property, out var v) ? v : null;
    }

    private static string? Text(object value) => value.ToString();
}
