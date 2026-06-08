using KitStack.Audit.Abstractions.Models;

namespace KitStack.Audit.Abstractions.Contracts;

/// <summary>
/// Destination for captured audit trails. Implementations persist trails to a backend
/// (EF Core, Mongo, file, in-memory). A sink must never throw in a way that breaks the
/// originating business transaction — log and swallow instead.
/// </summary>
public interface IAuditSink
{
    Task WriteAsync(IReadOnlyList<TrailDto> trails, CancellationToken cancellationToken = default);
}
