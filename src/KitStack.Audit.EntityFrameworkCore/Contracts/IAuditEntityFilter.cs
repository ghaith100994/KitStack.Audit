using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KitStack.Audit.EntityFrameworkCore.Contracts;

/// <summary>
/// First-pass filter deciding whether a tracked entry is a candidate for auditing.
/// Runs before the <c>[AuditDefinition]</c> + <c>IAuditableEntityRegistry</c> gate.
/// Register a custom implementation to restrict auditing (e.g. to aggregate roots).
/// </summary>
public interface IAuditEntityFilter
{
    bool ShouldAudit(EntityEntry entry);
}
