using KitStack.Audit.Abstractions.Enums;

namespace KitStack.Audit.Abstractions.Models;

/// <summary>
/// In-flight audit record produced by a capture source (e.g. the EF Core interceptor)
/// before it is handed to an <c>IAuditSink</c>. Holds values as raw dictionaries so each
/// sink can decide how to serialize/store them.
/// </summary>
public sealed class TrailDto
{
    public DefaultIdType Id { get; set; } = Guid.NewGuid();
    public DefaultIdType UserId { get; set; }

    /// <summary>Display name of the acting user, if the context accessor provides one.</summary>
    public string? UserName { get; set; }

    /// <summary>Tenant the change belongs to in multi-tenant applications.</summary>
    public string? TenantId { get; set; }

    /// <summary>Correlation/trace id linking the trail to the originating request.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Client IP address of the originating request.</summary>
    public string? IpAddress { get; set; }

    public string? TableName { get; set; }

    /// <summary>Flat string form of the primary key (joined when composite), for direct querying.</summary>
    public string? EntityId { get; set; }

    public string? Module { get; set; }

    /// <summary>Sub-module / feature area from <c>[AuditDefinition]</c>.</summary>
    public string? SubModule { get; set; }

    /// <summary>Capture source that produced the trail (e.g. "EntityFrameworkCore", "Manual").</summary>
    public string? Source { get; set; }

    /// <summary>
    /// Logical name of the owning/parent aggregate this change is also attributed to
    /// (see <c>[AuditParent]</c>), e.g. "Committee" for a CommitteeMember change.
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>Id of the owning/parent aggregate.</summary>
    public string? RelatedEntityId { get; set; }

    public TrailType Type { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public Dictionary<string, object?> KeyValues { get; } = new();
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public List<string> ModifiedProperties { get; } = new();
}
