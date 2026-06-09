using KitStack.Audit.Abstractions.Contracts;

namespace KitStack.Audit.Fakes.Sinks;

/// <summary>
/// An <see cref="IAuditableEntityRegistry"/> that treats everything as auditable.
/// </summary>
public sealed class AllowAllAuditableEntityRegistry : IAuditableEntityRegistry
{
    public Task<bool> IsAuditableAsync(string? module, string resource, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}
