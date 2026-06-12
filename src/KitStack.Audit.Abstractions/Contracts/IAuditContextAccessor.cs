namespace KitStack.Audit.Abstractions.Contracts;

/// <summary>
/// Supplies the acting user for an audit record, decoupled from any specific
/// application's current-user abstraction.
/// </summary>
public interface IAuditContextAccessor
{
    DefaultIdType UserId { get; }
    string? UserName { get; }

    /// <summary>Tenant the change belongs to in multi-tenant applications. Optional.</summary>
    string? TenantId => null;

    /// <summary>Correlation/trace id linking the trail to the originating request. Optional.</summary>
    string? CorrelationId => null;

    /// <summary>Client IP address of the originating request. Optional.</summary>
    string? IpAddress => null;
}
