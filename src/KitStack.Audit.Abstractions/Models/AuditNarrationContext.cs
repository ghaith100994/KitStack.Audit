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
    public string? PrimaryKey { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public IReadOnlyDictionary<string, object?> OldValues { get; init; }
        = new Dictionary<string, object?>();

    public IReadOnlyDictionary<string, object?> NewValues { get; init; }
        = new Dictionary<string, object?>();

    public IReadOnlyCollection<string> ModifiedProperties { get; init; }
        = Array.Empty<string>();
}
