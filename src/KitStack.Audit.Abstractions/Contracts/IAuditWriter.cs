using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Models;

namespace KitStack.Audit.Abstractions.Contracts;

/// <summary>
/// Manual/explicit capture source: record audit trails for changes that do not flow through
/// an automatic capture pipeline (background jobs, bulk SQL, external system calls, ...).
/// Implementations stamp the acting user and timestamp before forwarding to the
/// registered <see cref="IAuditSink"/>.
/// </summary>
public interface IAuditWriter
{
    /// <summary>Record a fully built trail. Missing user/timestamp fields are stamped from the audit context.</summary>
    Task RecordAsync(TrailDto trail, CancellationToken cancellationToken = default);

    /// <summary>Record a batch of trails in a single sink write.</summary>
    Task RecordAsync(IReadOnlyList<TrailDto> trails, CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience overload that builds the trail for you.
    /// </summary>
    /// <param name="entityName">Logical entity/table name the change applies to.</param>
    /// <param name="type">Create/Update/Delete.</param>
    /// <param name="entityId">Primary key of the affected record, if known.</param>
    /// <param name="oldValues">Values before the change (Update/Delete).</param>
    /// <param name="newValues">Values after the change (Create/Update).</param>
    /// <param name="module">Optional logical module; falls back to the configured default.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordAsync(
        string entityName,
        TrailType type,
        object? entityId = null,
        IReadOnlyDictionary<string, object?>? oldValues = null,
        IReadOnlyDictionary<string, object?>? newValues = null,
        string? module = null,
        CancellationToken cancellationToken = default);
}
