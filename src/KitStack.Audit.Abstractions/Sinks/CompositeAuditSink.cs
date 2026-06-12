using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Models;

namespace KitStack.Audit.Abstractions.Sinks;

/// <summary>
/// Fans a single write out to several sinks (e.g. relational + file). Every inner sink receives
/// every batch; per the <see cref="IAuditSink"/> contract each inner sink is responsible for
/// swallowing its own failures, so one failing destination does not starve the others.
/// </summary>
public sealed class CompositeAuditSink : IAuditSink, IActivitySink
{
    private readonly IReadOnlyList<IAuditSink> _sinks;

    public CompositeAuditSink(IEnumerable<IAuditSink> sinks)
    {
        ArgumentNullException.ThrowIfNull(sinks);
        _sinks = sinks.ToList();
    }

    public CompositeAuditSink(params IAuditSink[] sinks)
        : this((IEnumerable<IAuditSink>)sinks)
    {
    }

    public async Task WriteAsync(IReadOnlyList<TrailDto> trails, CancellationToken cancellationToken = default)
    {
        foreach (var sink in _sinks)
            await sink.WriteAsync(trails, cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteActivitiesAsync(IReadOnlyList<ActivityEvent> activities, CancellationToken cancellationToken = default)
    {
        foreach (var sink in _sinks)
            await sink.WriteActivitiesAsync(activities, cancellationToken).ConfigureAwait(false);
    }
}
