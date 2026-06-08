namespace KitStack.Audit.Abstractions.Contracts;

/// <summary>
/// Supplies the acting user for an audit record, decoupled from any specific
/// application's current-user abstraction.
/// </summary>
public interface IAuditContextAccessor
{
    DefaultIdType UserId { get; }
    string? UserName { get; }
}
