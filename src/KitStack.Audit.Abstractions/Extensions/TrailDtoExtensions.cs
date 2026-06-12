using System.Text.Json;
using System.Text.Json.Serialization;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Abstractions.Utilities;

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
            UserName = dto.UserName,
            TenantId = dto.TenantId,
            CorrelationId = dto.CorrelationId,
            IpAddress = dto.IpAddress,
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

    /// <summary>Whether <paramref name="propertyName"/> is in the modified-property list (Update trails).</summary>
    public static bool HasChanged(this TrailDto dto, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return dto.ModifiedProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>The pre-change value of <paramref name="propertyName"/>, converted to <typeparamref name="T"/>.</summary>
    public static T? GetOldValue<T>(this TrailDto dto, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return AuditValueReader.Get<T>(dto.OldValues, propertyName);
    }

    /// <summary>The post-change value of <paramref name="propertyName"/>, converted to <typeparamref name="T"/>.</summary>
    public static T? GetNewValue<T>(this TrailDto dto, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return AuditValueReader.Get<T>(dto.NewValues, propertyName);
    }

    private static string Serialize(object value)
        => JsonSerializer.Serialize(value, SerializerOptions);
}
