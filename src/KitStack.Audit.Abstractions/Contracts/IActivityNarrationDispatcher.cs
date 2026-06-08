using KitStack.Audit.Abstractions.Models;

namespace KitStack.Audit.Abstractions.Contracts;

/// <summary>
/// Routes a narration context to the first <see cref="IAuditNarrator"/> that can handle it.
/// </summary>
public interface IActivityNarrationDispatcher
{
    ActivityEvent? Narrate(AuditNarrationContext context);
}
