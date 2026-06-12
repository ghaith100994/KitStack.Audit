using KitStack.Audit.Abstractions.Enums;

namespace KitStack.Audit.Abstractions.Models;

/// <summary>
/// Snapshot of a single entity change passed to narrators so they can build an
/// <see cref="ActivityEvent"/> without depending on EF Core.
/// </summary>
public sealed class AuditNarrationContext
{
    public required Type EntityType { get; init; }
    public required TrailType TrailType { get; init; }

    public DefaultIdType UserId { get; init; }
    public string? UserName { get; init; }
    public string? TenantId { get; init; }
    public string? CorrelationId { get; init; }
    public string? Module { get; init; }

    /// <summary>Sub-module / feature area from <c>[AuditDefinition]</c>.</summary>
    public string? SubModule { get; init; }

    /// <summary>English display name from <c>[AuditDefinition]</c> (e.g. "Committee member").</summary>
    public string? EntityDisplayEn { get; init; }

    /// <summary>Arabic display name from <c>[AuditDefinition]</c>.</summary>
    public string? EntityDisplayAr { get; init; }

    /// <summary>Primary owning/parent aggregate name resolved from <c>[AuditParent]</c>, if any.</summary>
    public string? RelatedEntityType { get; init; }

    /// <summary>Primary owning/parent aggregate id resolved from <c>[AuditParent]</c>, if any.</summary>
    public string? RelatedEntityId { get; init; }

    /// <summary>Every parent aggregate the change is attributed to (multi-parent entities).</summary>
    public IReadOnlyList<AuditRelation> Relations { get; init; } = Array.Empty<AuditRelation>();

    public string? PrimaryKey { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public IReadOnlyDictionary<string, object?> OldValues { get; init; }
        = new Dictionary<string, object?>();

    public IReadOnlyDictionary<string, object?> NewValues { get; init; }
        = new Dictionary<string, object?>();

    public IReadOnlyCollection<string> ModifiedProperties { get; init; }
        = Array.Empty<string>();
}
