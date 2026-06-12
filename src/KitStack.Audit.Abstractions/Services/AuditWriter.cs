using System.Globalization;
using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Abstractions.Utilities;

namespace KitStack.Audit.Abstractions.Services;

/// <summary>
/// Default <see cref="IAuditWriter"/>: stamps user/tenant/correlation from the
/// <see cref="IAuditContextAccessor"/>, normalizes values with
/// <see cref="AuditValueConverter"/>, and forwards to the registered <see cref="IAuditSink"/>.
/// </summary>
public class AuditWriter : IAuditWriter
{
    private readonly IAuditSink _sink;
    private readonly IAuditContextAccessor _context;

    public AuditWriter(IAuditSink sink, IAuditContextAccessor context)
    {
        ArgumentNullException.ThrowIfNull(sink);
        ArgumentNullException.ThrowIfNull(context);
        _sink = sink;
        _context = context;
    }

    public Task RecordAsync(TrailDto trail, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trail);
        return RecordAsync(new[] { trail }, cancellationToken);
    }

    public Task RecordAsync(IReadOnlyList<TrailDto> trails, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trails);
        if (trails.Count == 0)
            return Task.CompletedTask;

        foreach (var trail in trails)
            Stamp(trail);

        return _sink.WriteAsync(trails, cancellationToken);
    }

    public Task RecordAsync(
        string entityName,
        TrailType type,
        object? entityId = null,
        IReadOnlyDictionary<string, object?>? oldValues = null,
        IReadOnlyDictionary<string, object?>? newValues = null,
        string? module = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);

        var trail = new TrailDto
        {
            TableName = entityName,
            Type = type,
            Module = module,
        };

        if (entityId is not null)
            trail.KeyValues["Id"] = entityId;

        CopyNormalized(oldValues, trail.OldValues);
        CopyNormalized(newValues, trail.NewValues);

        if (type == TrailType.Update && oldValues is not null && newValues is not null)
        {
            foreach (var key in newValues.Keys)
            {
                if (!oldValues.TryGetValue(key, out var old) || !Equals(old, newValues[key]))
                    trail.ModifiedProperties.Add(key);
            }
        }

        return RecordAsync(trail, cancellationToken);
    }

    private void Stamp(TrailDto trail)
    {
        if (trail.UserId == default)
            trail.UserId = _context.UserId;
        trail.UserName ??= _context.UserName;
        trail.TenantId ??= _context.TenantId;
        trail.CorrelationId ??= _context.CorrelationId;
        trail.IpAddress ??= _context.IpAddress;
    }

    private static void CopyNormalized(
        IReadOnlyDictionary<string, object?>? source, Dictionary<string, object?> target)
    {
        if (source is null)
            return;

        foreach (var (key, value) in source)
            target[key] = AuditValueConverter.Normalize(value, CultureInfo.InvariantCulture);
    }
}
