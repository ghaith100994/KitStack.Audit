namespace KitStack.Audit.Abstractions.Models;

/// <summary>
/// Links a trail/activity to an owning or related aggregate. A change can carry several
/// relations — e.g. adding a Person produces relations to both their Organization and their
/// Department — so each parent's profile feed can show the change.
/// Produced automatically from <c>[AuditParent]</c>-annotated foreign keys.
/// </summary>
public class AuditRelation
{
    public AuditRelation()
    {
    }

    public AuditRelation(string entityType, string? entityId, string? entityName = null)
    {
        EntityType = entityType;
        EntityId = entityId;
        EntityName = entityName;
    }

    /// <summary>Logical name of the related aggregate (e.g. "Organization", "Department").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Id of the related aggregate.</summary>
    public string? EntityId { get; set; }

    /// <summary>Optional display name of the related aggregate (e.g. the department's name).</summary>
    public string? EntityName { get; set; }
}
