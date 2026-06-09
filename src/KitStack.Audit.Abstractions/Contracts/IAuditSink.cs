using KitStack.Audit.Abstractions.Models;

namespace KitStack.Audit.Abstractions.Contracts;

/// <summary>
/// Destination for captured audit data. Implementations persist to a backend
/// (EF Core, Mongo, file, in-memory). A sink must never throw in a way that breaks the
/// originating business transaction — log and swallow instead.
/// </summary>
public interface IAuditSink
{
    /// <summary>Persist raw audit trails.</summary>
    Task WriteAsync(IReadOnlyList<TrailDto> trails, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persist human-readable activity events produced by narration. Optional:
    /// sinks that don't store activities can leave the default no-op.
    /// </summary>
    Task WriteActivitiesAsync(IReadOnlyList<ActivityEvent> activities, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
