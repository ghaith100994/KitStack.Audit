using System.Text.Json;
using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Models;

namespace KitStack.Audit.Abstractions.Extensions;

/// <summary>
/// Reads the JSON payload columns of a persisted <see cref="AuditTrail"/> back into
/// structured data, for building audit-log screens and reports on top of the store.
/// </summary>
public static class AuditTrailExtensions
{
    /// <summary>The <see cref="AuditTrail.Operation"/> string parsed back to a <see cref="TrailType"/>.</summary>
    public static TrailType? GetTrailType(this AuditTrail trail)
    {
        ArgumentNullException.ThrowIfNull(trail);
        return Enum.TryParse<TrailType>(trail.Operation, ignoreCase: true, out var type) ? type : null;
    }

    /// <summary>The deserialized <see cref="AuditTrail.PreviousValues"/> payload (empty for creates).</summary>
    public static IReadOnlyDictionary<string, JsonElement> GetPreviousValues(this AuditTrail trail)
    {
        ArgumentNullException.ThrowIfNull(trail);
        return DeserializeMap(trail.PreviousValues);
    }

    /// <summary>The deserialized <see cref="AuditTrail.NewValues"/> payload (empty for deletes).</summary>
    public static IReadOnlyDictionary<string, JsonElement> GetNewValues(this AuditTrail trail)
    {
        ArgumentNullException.ThrowIfNull(trail);
        return DeserializeMap(trail.NewValues);
    }

    /// <summary>The deserialized <see cref="AuditTrail.PrimaryKey"/> payload.</summary>
    public static IReadOnlyDictionary<string, JsonElement> GetPrimaryKey(this AuditTrail trail)
    {
        ArgumentNullException.ThrowIfNull(trail);
        return DeserializeMap(trail.PrimaryKey);
    }

    /// <summary>The deserialized <see cref="AuditTrail.ModifiedProperties"/> list.</summary>
    public static IReadOnlyList<string> GetModifiedProperties(this AuditTrail trail)
    {
        ArgumentNullException.ThrowIfNull(trail);
        if (string.IsNullOrWhiteSpace(trail.ModifiedProperties))
            return Array.Empty<string>();

        return JsonSerializer.Deserialize<List<string>>(trail.ModifiedProperties) ?? (IReadOnlyList<string>)Array.Empty<string>();
    }

    private static IReadOnlyDictionary<string, JsonElement> DeserializeMap(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, JsonElement>();

        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)
            ?? new Dictionary<string, JsonElement>();
    }
}
