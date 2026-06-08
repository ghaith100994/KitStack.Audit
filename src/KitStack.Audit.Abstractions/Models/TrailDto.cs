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
    public string? TableName { get; set; }
    public string? Module { get; set; }
    public TrailType Type { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public Dictionary<string, object?> KeyValues { get; } = new();
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public List<string> ModifiedProperties { get; } = new();
}
