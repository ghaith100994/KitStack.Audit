using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using KitStack.Audit.Abstractions.Attributes;
using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Abstractions.Options;
using KitStack.Audit.Abstractions.Utilities;
using KitStack.Audit.EntityFrameworkCore.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KitStack.Audit.EntityFrameworkCore.Interceptors;

/// <summary>
/// Captures Added/Modified/Deleted entities as <see cref="TrailDto"/> on save and forwards
/// the survivors to the registered <see cref="IAuditSink"/>.
///
/// Lifecycle:
///   SavingChanges  -> snapshot trails (change tracker is still intact, original values available)
///   SavedChanges   -> apply the auditable gate and write to the sink (business save succeeded)
///   SaveChangesFailed -> discard the snapshot (no orphan trails for rolled-back work)
///
/// Register as <c>Scoped</c> and attach via <c>options.AddInterceptors(sp.GetRequiredService&lt;...&gt;())</c>.
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    // Keyed by DbContext so a singleton/shared interceptor stays correct; entries are GC'd with the context.
    private static readonly ConditionalWeakTable<DbContext, List<PendingTrail>> Pending = new();

    private readonly IAuditContextAccessor _user;
    private readonly IAuditSink _sink;
    private readonly IAuditEntityFilter _filter;
    private readonly AuditOptions _options;
    private readonly IAuditableEntityRegistry? _registry;
    private readonly ILogger<AuditSaveChangesInterceptor>? _logger;

    public AuditSaveChangesInterceptor(
        IAuditContextAccessor user,
        IAuditSink sink,
        IAuditEntityFilter filter,
        IOptions<AuditOptions> options,
        IAuditableEntityRegistry? registry = null,
        ILogger<AuditSaveChangesInterceptor>? logger = null)
    {
        _user = user;
        _sink = sink;
        _filter = filter;
        _options = options.Value;
        _registry = registry;
        _logger = logger;
    }

    // ── capture (before the save) ─────────────────────────────────────────

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // ── write (after the save succeeds) ───────────────────────────────────

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        // Sync path: block on the async write. Prefer SaveChangesAsync where possible.
        FlushAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await FlushAsync(eventData.Context, cancellationToken).ConfigureAwait(false);
        return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    // ── discard (on failure) ──────────────────────────────────────────────

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context is not null)
            Pending.Remove(eventData.Context);
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            Pending.Remove(eventData.Context);
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    // ── internals ─────────────────────────────────────────────────────────

    private void Capture(DbContext? context)
    {
        if (context is not IAuditableDbContext { EnableAuditing: true } auditable)
            return;

        context.ChangeTracker.DetectChanges();

        var module = auditable.AuditModule ?? _options.DefaultModule;
        var culture = CultureInfo.InvariantCulture;
        var utcNow = DateTime.UtcNow;
        var userId = _user.UserId;

        var captured = new List<PendingTrail>();

        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;
            if (!_filter.ShouldAudit(entry))
                continue;

            var entityType = entry.Entity.GetType();
            var resource = entityType.GetCustomAttribute<AuditDefinitionAttribute>(inherit: true)?.Resource;

            var trail = new TrailDto
            {
                TableName = entityType.Name,
                UserId = userId,
                DateTime = utcNow,
                Module = module,
            };

            foreach (var property in entry.Properties)
            {
                // Store-generated keys are temporary until after save; skip (use client-generated keys to capture them).
                if (property.IsTemporary)
                    continue;

                var name = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    trail.KeyValues[name] = property.CurrentValue;
                    continue;
                }

                var original = AuditValueConverter.Normalize(property.OriginalValue, culture);
                var current = AuditValueConverter.Normalize(property.CurrentValue, culture);

                switch (entry.State)
                {
                    case EntityState.Added:
                        trail.Type = TrailType.Create;
                        trail.NewValues[name] = current;
                        break;

                    case EntityState.Deleted:
                        trail.Type = TrailType.Delete;
                        trail.OldValues[name] = original;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified &&
                            property.OriginalValue?.Equals(property.CurrentValue) != true)
                        {
                            trail.Type = TrailType.Update;
                            trail.ModifiedProperties.Add(name);
                            trail.OldValues[name] = original;
                            trail.NewValues[name] = current;
                        }
                        break;
                }
            }

            // A modified row where no audited property actually changed -> nothing to record.
            if (entry.State == EntityState.Modified && trail.ModifiedProperties.Count == 0)
                continue;

            captured.Add(new PendingTrail(trail, resource));
        }

        if (captured.Count == 0)
            return;

        Pending.AddOrUpdate(context, captured);
    }

    private async Task FlushAsync(DbContext? context, CancellationToken ct)
    {
        if (context is null || !Pending.TryGetValue(context, out var captured))
            return;

        Pending.Remove(context);

        var module = (context as IAuditableDbContext)?.AuditModule ?? _options.DefaultModule;

        var toWrite = new List<TrailDto>(captured.Count);
        foreach (var pending in captured)
        {
            if (_options.EnforceAuditableSetting && _registry is not null && pending.Resource is not null)
            {
                bool auditable;
                try
                {
                    auditable = await _registry.IsAuditableAsync(module, pending.Resource, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Auditable check failed for {Resource}; skipping.", pending.Resource);
                    continue;
                }

                if (!auditable)
                    continue;
            }

            toWrite.Add(pending.Trail);
        }

        if (toWrite.Count == 0)
            return;

        try
        {
            await _sink.WriteAsync(toWrite, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // The business transaction has already committed; auditing must never surface here.
            _logger?.LogError(ex, "Failed to write {Count} audit trails.", toWrite.Count);
        }
    }

    private sealed record PendingTrail(TrailDto Trail, string? Resource);
}
