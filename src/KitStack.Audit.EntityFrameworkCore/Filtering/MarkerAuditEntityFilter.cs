using KitStack.Audit.EntityFrameworkCore.Contracts;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KitStack.Audit.EntityFrameworkCore.Filtering;

/// <summary>
/// Audits only entities assignable to <typeparamref name="TMarker"/>.
/// Use this to restrict auditing to aggregate roots, e.g.
/// <c>MarkerAuditEntityFilter&lt;IAggregateRoot&gt;</c>.
/// </summary>
public sealed class MarkerAuditEntityFilter<TMarker> : IAuditEntityFilter
    where TMarker : class
{
    public bool ShouldAudit(EntityEntry entry) => entry.Entity is TMarker;
}
