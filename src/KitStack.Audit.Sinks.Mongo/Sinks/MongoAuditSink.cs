using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Extensions;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Sinks.Mongo.Persistence;
using Microsoft.Extensions.Logging;

namespace KitStack.Audit.Sinks.Mongo.Sinks;

/// <summary>
/// Persists trails (and, optionally, activities) to MongoDB through <see cref="MongoAuditDbContext"/>.
/// Errors are logged and swallowed — the originating business transaction has already committed.
/// </summary>
public sealed class MongoAuditSink : IAuditSink, IActivitySink
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

    public async Task WriteActivitiesAsync(IReadOnlyList<ActivityEvent> activities, CancellationToken cancellationToken = default)
    {
        if (activities is null || activities.Count == 0)
            return;

        try
        {
            _context.Activities.AddRange(activities);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while saving {Count} activity events to MongoDB (EF Core).", activities.Count);
        }
    }
}
