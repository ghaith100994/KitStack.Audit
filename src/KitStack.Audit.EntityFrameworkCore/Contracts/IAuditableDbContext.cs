namespace KitStack.Audit.EntityFrameworkCore.Contracts;

/// <summary>
/// Implemented by application DbContexts that participate in auditing.
/// The interceptor only captures changes for contexts where <see cref="EnableAuditing"/> is true,
/// and tags every trail with <see cref="AuditModule"/>.
/// The audit store's own DbContext should return <c>false</c> to avoid auditing the audit.
/// </summary>
public interface IAuditableDbContext
{
    /// <summary>When false, the interceptor ignores this context entirely.</summary>
    bool EnableAuditing { get; }

    /// <summary>Optional logical module name stamped onto every captured trail.</summary>
    string? AuditModule { get; }
}
