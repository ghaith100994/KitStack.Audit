using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Models;

namespace KitStack.Audit.AspNetCore.Narration;

/// <summary>
/// Routes a narration context to the first registered <see cref="IAuditNarrator"/> that can handle
/// the entity type. Lifted from the original ERP dispatcher.
/// </summary>
public sealed class ActivityNarrationDispatcher : IActivityNarrationDispatcher
{
    private readonly IReadOnlyList<IAuditNarrator> _narrators;

    public ActivityNarrationDispatcher(IEnumerable<IAuditNarrator> narrators)
        => _narrators = narrators.ToList();

    public ActivityEvent? Narrate(AuditNarrationContext context)
        => _narrators.FirstOrDefault(n => n.CanNarrate(context.EntityType))?.Narrate(context);
}
