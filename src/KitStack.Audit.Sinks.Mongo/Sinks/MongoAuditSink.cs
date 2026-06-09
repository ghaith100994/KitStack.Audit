using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Extensions;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Sinks.Mongo.Persistence;
using Microsoft.Extensions.Logging;

namespace KitStack.Audit.Sinks.Mongo.Sinks;

/// <summary>
/// Persists captured trails to MongoDB through <see cref="MongoAuditDbContext"/> (EF Core provider).
/// Each <see cref="TrailDto"/> is flattened to an <see cref="AuditTrail"/> document.
/// Errors are logged and swallowed — the originating business transaction has already committed.
/// </summary>
public sealed class MongoAuditSink : IAuditSink
{
    private readonly MongoAuditDbContext _context;
    private readonly ILogger<MongoAuditSink>? _logger;

    public MongoAuditSink(MongoAuditDbContext context, ILogger<MongoAuditSink>? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task WriteAsync(IReadOnlyList<TrailDto> trails, CancellationToken cancellationToken = default)
    {
        if (trails is null || trails.Count == 0)
            return;

        try
        {
            var entities = new List<AuditTrail>(trails.Count);
            foreach (var dto in trails)
                entities.Add(dto.ToAuditTrail());

            _context.AuditTrails.AddRange(entities);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while saving {Count} audit trails to MongoDB (EF Core).", trails.Count);
        }
    }
}
