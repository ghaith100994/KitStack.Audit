namespace KitStack.Audit.Abstractions.Contracts;

/// <summary>
/// Decides whether a given resource (within a module) should be audited.
/// Back this with settings, a cache, or a static allow-list.
/// </summary>
public interface IAuditableEntityRegistry
{
    Task<bool> IsAuditableAsync(string? module, string resource, CancellationToken cancellationToken = default);
}
