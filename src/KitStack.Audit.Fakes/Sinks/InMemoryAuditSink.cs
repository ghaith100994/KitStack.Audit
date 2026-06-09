using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Fakes.Contracts;

namespace KitStack.Audit.Fakes.Sinks;

/// <summary>
/// An in-memory sink that records trails and activities. Resolve it as
/// <see cref="IFakeAuditStore"/> to assert what was captured.
/// </summary>
public sealed class InMemoryAuditSink : IAuditSink, IActivitySink, IFakeAuditStore
{
    private readonly List<TrailDto> _trails = new();
    private readonly List<ActivityEvent> _activities = new();
    private readonly Lock _gate = new();

    public IReadOnlyList<TrailDto> Trails
    {
        get { lock (_gate) return _trails.ToList(); }
    }

    public IReadOnlyList<ActivityEvent> Activities
    {
        get { lock (_gate) return _activities.ToList(); }
    }

    public Task WriteAsync(IReadOnlyList<TrailDto> trails, CancellationToken cancellationToken = default)
    {
        if (trails is { Count: > 0 })
            lock (_gate) _trails.AddRange(trails);
        return Task.CompletedTask;
    }

    public Task WriteActivitiesAsync(IReadOnlyList<ActivityEvent> activities, CancellationToken cancellationToken = default)
    {
        if (activities is { Count: > 0 })
            lock (_gate) _activities.AddRange(activities);
        return Task.CompletedTask;
    }

    public void Clear()
    {
        lock (_gate)
        {
            _trails.Clear();
            _activities.Clear();
        }
    }
}
