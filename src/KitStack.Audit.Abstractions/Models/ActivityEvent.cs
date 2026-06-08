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
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public TrailType Action { get; set; }

    /// <summary>Short, human-readable summary (e.g. "Order #123 was approved").</summary>
    public string? Title { get; set; }

    /// <summary>Optional longer description.</summary>
    public string? Description { get; set; }

    public string? Module { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public IDictionary<string, string>? Metadata { get; set; }
}
