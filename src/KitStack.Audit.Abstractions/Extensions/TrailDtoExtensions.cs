using System.Text.Json;
using System.Text.Json.Serialization;
using KitStack.Audit.Abstractions.Models;

namespace KitStack.Audit.Abstractions.Extensions;

/// <summary>
/// Maps an in-flight <see cref="TrailDto"/> to the flat, persistence-ready
/// <see cref="AuditTrail"/> used by relational sinks.
/// </summary>
public static class TrailDtoExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    public static AuditTrail ToAuditTrail(this TrailDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new AuditTrail
        {
            Id = dto.Id,
            UserId = dto.UserId,
            Operation = dto.Type.ToString(),
            Entity = dto.TableName,
            Module = dto.Module,
            DateTime = dto.DateTime,
            PrimaryKey = dto.KeyValues.Count == 0 ? null : Serialize(dto.KeyValues),
            PreviousValues = dto.OldValues.Count == 0 ? null : Serialize(dto.OldValues),
            NewValues = dto.NewValues.Count == 0 ? null : Serialize(dto.NewValues),
            ModifiedProperties = dto.ModifiedProperties.Count == 0 ? null : Serialize(dto.ModifiedProperties),
        };
    }

    private static string Serialize(object value)
        => JsonSerializer.Serialize(value, SerializerOptions);
}
