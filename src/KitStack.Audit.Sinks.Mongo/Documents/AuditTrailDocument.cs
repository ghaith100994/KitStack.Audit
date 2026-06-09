using KitStack.Audit.Abstractions.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace KitStack.Audit.Sinks.Mongo.Documents;

/// <summary>
/// MongoDB document for an audit trail. Unlike the relational sink (which flattens values to
/// JSON strings), this keeps the key/old/new values as native embedded documents.
/// </summary>
public sealed class AuditTrailDocument
{
    [BsonId]
    public DefaultIdType Id { get; set; }

    public DefaultIdType UserId { get; set; }
    public string? TableName { get; set; }
    public string? Module { get; set; }

    /// <summary>The <see cref="Abstractions.Enums.TrailType"/> as a string.</summary>
    public string? Type { get; set; }

    public DateTime DateTime { get; set; }

    public Dictionary<string, object?> KeyValues { get; set; } = new();
    public Dictionary<string, object?> OldValues { get; set; } = new();
    public Dictionary<string, object?> NewValues { get; set; } = new();
    public List<string> ModifiedProperties { get; set; } = new();

    public static AuditTrailDocument From(TrailDto dto) => new()
    {
        Id = dto.Id,
        UserId = dto.UserId,
        TableName = dto.TableName,
        Module = dto.Module,
        Type = dto.Type.ToString(),
        DateTime = dto.DateTime,
        KeyValues = dto.KeyValues,
        OldValues = dto.OldValues,
        NewValues = dto.NewValues,
        ModifiedProperties = dto.ModifiedProperties,
    };
}
