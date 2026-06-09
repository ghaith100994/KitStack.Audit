using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Extensions;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Sinks.EntityFrameworkCore.Persistence;
using Microsoft.Extensions.Logging;

namespace KitStack.Audit.Sinks.EntityFrameworkCore.Sinks;

/// <summary>
/// Persists trails (and, optionally, activities) to a relational store via <see cref="AuditDbContext"/>.
/// Errors are logged and swallowed — the originating business transaction has already committed.
/// </summary>
public sealed class EfCoreAuditSink : IAuditSink, IActivitySink
{
    private readonly AuditDbContext _context;
    private readonly ILogger<EfCoreAuditSink>? _logger;

    public EfCoreAuditSink(AuditDbContext context, ILogger<EfCoreAuditSink>? logger = null)
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

            await _context.AuditTrails.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while saving {Count} audit trails.", trails.Count);
        }
    }

    public async Task WriteActivitiesAsync(IReadOnlyList<ActivityEvent> activities, CancellationToken cancellationToken = default)
    {
        if (activities is null || activities.Count == 0)
            return;

        try
        {
            await _context.Activities.AddRangeAsync(activities, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while saving {Count} activity events.", activities.Count);
        }
    }
}
