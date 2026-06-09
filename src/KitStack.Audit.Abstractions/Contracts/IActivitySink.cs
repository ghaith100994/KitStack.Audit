using KitStack.Audit.Abstractions.Models;

namespace KitStack.Audit.Abstractions.Contracts;

/// <summary>
/// Destination for human-readable <see cref="ActivityEvent"/>s produced by narration.
/// Optional and independent of <see cref="IAuditSink"/>; a sink may implement both.
/// Like the trail sink, it must never throw in a way that breaks the business transaction.
/// </summary>
public interface IActivitySink
{
    Task WriteActivitiesAsync(IReadOnlyList<ActivityEvent> activities, CancellationToken cancellationToken = default);
}
