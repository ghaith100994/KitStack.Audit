using System.Collections;
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
/// Captures Added/Modified/Deleted entities as <see cref="TrailDto"/> on save and forwards them
/// to the registered <see cref="IAuditSink"/>. If an <see cref="IActivityNarrationDispatcher"/>
/// and an <see cref="IActivitySink"/> are also registered, each surviving change is narrated into
/// an <see cref="ActivityEvent"/> and written too.
///
/// Lifecycle:
///   SavingChanges      -> snapshot trails (change tracker intact, original values available)
///   SavedChanges       -> apply the auditable gate, narrate, then write to the sinks
///   SaveChangesFailed  -> discard the snapshot (no orphan trails for rolled-back work)
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly ConditionalWeakTable<DbContext, List<PendingTrail>> Pending = new();

    private readonly IAuditContextAccessor _user;
    private readonly IAuditSink _sink;
    private readonly IAuditEntityFilter _filter;
    private readonly AuditOptions _options;
    private readonly IAuditableEntityRegistry? _registry;
    private readonly IActivityNarrationDispatcher? _narration;
    private readonly IActivitySink? _activitySink;
    private readonly ILogger<AuditSaveChangesInterceptor>? _logger;

    public AuditSaveChangesInterceptor(
        IAuditContextAccessor user,
        IAuditSink sink,
        IAuditEntityFilter filter,
        IOptions<AuditOptions> options,
        IAuditableEntityRegistry? registry = null,
        IActivityNarrationDispatcher? narration = null,
        IActivitySink? activitySink = null,
        ILogger<AuditSaveChangesInterceptor>? logger = null)
    {
        _user = user;
        _sink = sink;
        _filter = filter;
        _options = options.Value;
        _registry = registry;
        _narration = narration;
        _activitySink = activitySink;
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

            // Resolve the trail type once upfront — avoids it remaining as default(TrailType)
            // if every property in the loop is skipped (IsTemporary, IsPrimaryKey, etc.)
            var trailType = entry.State switch
            {
                EntityState.Added => TrailType.Create,
                EntityState.Deleted => TrailType.Delete,
                EntityState.Modified => TrailType.Update,
                _ => TrailType.Create,
            };

            var trail = new TrailDto
            {
                TableName = entityType.Name,
                UserId = userId,
                DateTime = utcNow,
                Module = module,
                Type = trailType,
            };

            foreach (var property in entry.Properties)
            {
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
                        trail.NewValues[name] = current;
                        break;

                    case EntityState.Deleted:
                        trail.OldValues[name] = original;
                        break;

                    case EntityState.Modified:
                        // Use structural equality so collection-typed columns
                        // (e.g. arrays/lists mapped via JSON columns or value converters)
                        // are not reported as modified when their content is unchanged.
                        if (property.IsModified && !AreValuesEqual(property.OriginalValue, property.CurrentValue))
                        {
                            trail.ModifiedProperties.Add(name);
                            trail.OldValues[name] = original;
                            trail.NewValues[name] = current;
                        }
                        break;
                }
            }

            // Skip Modified trails that turned out to have no real changes
            // (e.g. only collection properties that are structurally equal).
            if (entry.State == EntityState.Modified && trail.ModifiedProperties.Count == 0)
                continue;

            captured.Add(new PendingTrail(trail, resource, entityType));
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

        var trailsToWrite = new List<TrailDto>(captured.Count);
        var activities = new List<ActivityEvent>();

        foreach (var pending in captured)
        {
            if (_options.EnforceAuditableSetting && _registry is not null && pending.Resource is not null)
            {
                bool isAuditable;
                try
                {
                    isAuditable = await _registry.IsAuditableAsync(module, pending.Resource, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Auditable check failed for {Resource}; skipping.", pending.Resource);
                    continue;
                }

                if (!isAuditable)
                    continue;
            }

            trailsToWrite.Add(pending.Trail);

            if (_narration is not null)
            {
                var activity = _narration.Narrate(ToNarrationContext(pending));
                if (activity is not null)
                    activities.Add(activity);
            }
        }

        if (trailsToWrite.Count > 0)
        {
            try
            {
                await _sink.WriteAsync(trailsToWrite, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to write {Count} audit trail(s).", trailsToWrite.Count);
            }
        }

        if (_activitySink is not null && activities.Count > 0)
        {
            try
            {
                await _activitySink.WriteActivitiesAsync(activities, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to write {Count} activity event(s).", activities.Count);
            }
        }
    }

    private AuditNarrationContext ToNarrationContext(PendingTrail pending)
    {
        var trail = pending.Trail;
        return new AuditNarrationContext
        {
            EntityType = pending.EntityType,
            TrailType = trail.Type,
            UserId = trail.UserId,
            UserName = _user.UserName,
            Module = trail.Module,
            PrimaryKey = trail.KeyValues.Count == 0 ? null : string.Join(",", trail.KeyValues.Values),
            OldValues = trail.OldValues,
            NewValues = trail.NewValues,
            ModifiedProperties = trail.ModifiedProperties,
            Timestamp = trail.DateTime,
        };
    }

    /// <summary>
    /// Structural equality check that handles collection-typed property values correctly.
    /// <para>
    /// EF Core marks a property as <c>IsModified = true</c> whenever the change tracker
    /// detects the reference has changed, even if the underlying sequence is identical.
    /// <see cref="List{T}"/> and arrays do not override <see cref="object.Equals"/>, so a
    /// plain <c>a?.Equals(b)</c> call always returns <c>false</c> for those types —
    /// producing false positives in <c>ModifiedProperties</c>.
    /// </para>
    /// </summary>
    private static bool AreValuesEqual(object? a, object? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;

        // Fast path for primitive / value-type equality
        if (a.Equals(b)) return true;

        // Structural equality for any enumerable (arrays, List<T>, etc.)
        if (a is IEnumerable ea && b is IEnumerable eb)
        {
            var listA = ea.Cast<object?>().ToList();
            var listB = eb.Cast<object?>().ToList();

            if (listA.Count != listB.Count)
                return false;

            return listA.Zip(listB, (x, y) => AreValuesEqual(x, y)).All(eq => eq);
        }

        return false;
    }

    private sealed record PendingTrail(TrailDto Trail, string? Resource, Type EntityType);
}
