using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Models;

namespace KitStack.Audit.Abstractions.Services;

/// <summary>
/// Convenience base for a narrator bound to a single entity type.
/// Override <see cref="Narrate"/> to map a change into an <see cref="ActivityEvent"/>.
/// </summary>
public abstract class AuditNarratorBase<TEntity> : IAuditNarrator
    where TEntity : class
{
    public virtual bool CanNarrate(Type entityType) => entityType == typeof(TEntity);

    public abstract ActivityEvent? Narrate(AuditNarrationContext context);
}
