using KitStack.Audit.Abstractions.Enums;

namespace KitStack.Audit.Abstractions.Models;

/// <summary>
/// A human-readable activity produced by an <c>IAuditNarrator</c> from a change.
/// Persisted alongside (or instead of) raw audit trails for activity feeds.
/// </summary>
public class ActivityEvent
{
    public DefaultIdType Id { get; set; } = Guid.NewGuid();
    public DefaultIdType UserId { get; set; }

    /// <summary>Display name of the acting user, if available.</summary>
    public string? UserName { get; set; }

    /// <summary>Tenant the activity belongs to in multi-tenant applications.</summary>
    public string? TenantId { get; set; }

    /// <summary>Correlation/trace id linking the activity to the originating request.</summary>
    public string? CorrelationId { get; set; }

    public string? EntityType { get; set; }
    public string? EntityId { get; set; }

    /// <summary>
    /// Logical name of the owning/parent aggregate this activity should also appear under.
    /// Example: a CommitteeMember activity with RelatedEntityType "Committee" shows up on the
    /// committee's profile feed even though the change happened on the member row.
    /// Stamped automatically from <c>[AuditParent]</c> when narration runs through the interceptor.
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>Id of the owning/parent aggregate.</summary>
    public string? RelatedEntityId { get; set; }

    /// <summary>Optional display name of the related aggregate (e.g. the committee's name).</summary>
    public string? RelatedEntityName { get; set; }

    public TrailType Action { get; set; }

    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? SubtitleEn { get; set; }
    public string? SubtitleAr { get; set; }
    public string? Icon { get; set; }
    public string? Severity { get; set; }

    public string? Module { get; set; }

    /// <summary>Sub-module / feature area from <c>[AuditDefinition]</c>.</summary>
    public string? SubModule { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public IDictionary<string, string>? Metadata { get; set; }
}
