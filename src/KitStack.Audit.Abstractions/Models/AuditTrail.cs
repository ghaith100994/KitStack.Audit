namespace KitStack.Audit.Abstractions.Models;

/// <summary>
/// Flattened, persistence-ready audit record. Values are serialized to strings so
/// the type carries no provider (EF/Mongo) attributes and can live in abstractions.
/// </summary>
public class AuditTrail
{
    public DefaultIdType Id { get; set; }
    public DefaultIdType UserId { get; set; }

    /// <summary>Display name of the acting user, if available.</summary>
    public string? UserName { get; set; }

    /// <summary>Tenant the change belongs to in multi-tenant applications.</summary>
    public string? TenantId { get; set; }

    /// <summary>Correlation/trace id linking the trail to the originating request.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Client IP address of the originating request.</summary>
    public string? IpAddress { get; set; }

    /// <summary>The <see cref="Enums.TrailType"/> as a string (Create/Update/Delete).</summary>
    public string? Operation { get; set; }

    /// <summary>The entity/table name.</summary>
    public string? Entity { get; set; }

    /// <summary>Optional logical module name.</summary>
    public string? Module { get; set; }

    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    /// <summary>JSON of the old values (null for creates).</summary>
    public string? PreviousValues { get; set; }

    /// <summary>JSON of the new values (null for deletes).</summary>
    public string? NewValues { get; set; }

    /// <summary>JSON array of changed property names.</summary>
    public string? ModifiedProperties { get; set; }

    /// <summary>JSON of the primary key value(s).</summary>
    public string? PrimaryKey { get; set; }
}
