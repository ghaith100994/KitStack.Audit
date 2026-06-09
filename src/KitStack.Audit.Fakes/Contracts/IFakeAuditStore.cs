using KitStack.Audit.Abstractions.Models;

namespace KitStack.Audit.Fakes.Contracts;

/// <summary>
/// Read access to everything the in-memory sink has captured, for test assertions.
/// </summary>
public interface IFakeAuditStore
{
    /// <summary>Snapshot of all trails written so far.</summary>
    IReadOnlyList<TrailDto> Trails { get; }

    /// <summary>Snapshot of all narrated activities written so far.</summary>
    IReadOnlyList<ActivityEvent> Activities { get; }

    /// <summary>Remove all captured trails and activities.</summary>
    void Clear();
}
