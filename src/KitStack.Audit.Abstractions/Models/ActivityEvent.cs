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
    public TrailType Action { get; set; }

    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? SubtitleEn { get; set; }
    public string? SubtitleAr { get; set; }
    public string? Icon { get; set; }
    public string? Severity { get; set; }

    public string? Module { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public IDictionary<string, string>? Metadata { get; set; }
}
