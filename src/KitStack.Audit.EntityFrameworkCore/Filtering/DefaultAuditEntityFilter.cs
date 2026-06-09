using KitStack.Audit.EntityFrameworkCore.Contracts;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KitStack.Audit.EntityFrameworkCore.Filtering;

/// <summary>
/// Default filter: every changed entity is a candidate. The attribute + registry gate
/// (and any custom filter) narrows it down from there.
/// </summary>
public sealed class DefaultAuditEntityFilter : IAuditEntityFilter
{
    public bool ShouldAudit(EntityEntry entry) => true;
}
