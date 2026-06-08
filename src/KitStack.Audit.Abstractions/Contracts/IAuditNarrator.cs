using KitStack.Audit.Abstractions.Models;

namespace KitStack.Audit.Abstractions.Contracts;

/// <summary>
/// Converts a raw entity change into a human-readable <see cref="ActivityEvent"/>.
/// Register one narrator per entity type you want to narrate.
/// </summary>
public interface IAuditNarrator
{
    bool CanNarrate(Type entityType);
    ActivityEvent? Narrate(AuditNarrationContext context);
}
