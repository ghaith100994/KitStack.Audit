using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Options;
using KitStack.Audit.AspNetCore.Narration;
using KitStack.Audit.EntityFrameworkCore.Extensions;
using KitStack.Audit.Fakes.Extensions;
using KitStack.Audit.Sinks.EntityFrameworkCore.Extensions;
using KitStack.Audit.Sinks.Mongo.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// One-call setup for KitStack.Audit. Binds <c>AuditOptions</c>, registers the EF Core capture
/// source, and selects a sink based on <c>Audit:Sink</c> ("efcore", "mongo", or "fake").
///
/// The application is still responsible for:
///   • registering an <see cref="IAuditContextAccessor"/> (the current user), and
///   • attaching the interceptor to each audited DbContext:
///     <code>o.AddInterceptors(sp.GetRequiredService&lt;AuditSaveChangesInterceptor&gt;())</code>
/// </summary>
public static class AuditServiceCollectionExtensions
{
    /// <summary>Audit every changed entity (refined by <c>[AuditDefinition]</c> + registry).</summary>
    public static IServiceCollection AddKitStackAudit(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? efSinkProvider = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddKitStackAuditCapture();
        return ConfigureAudit(services, configuration, efSinkProvider);
    }

    /// <summary>
    /// Restrict auditing to entities assignable to <typeparamref name="TAuditMarker"/>
    /// (e.g. an aggregate-root marker interface).
    /// </summary>
    public static IServiceCollection AddKitStackAudit<TAuditMarker>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? efSinkProvider = null)
        where TAuditMarker : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddKitStackAuditCapture<TAuditMarker>();
        return ConfigureAudit(services, configuration, efSinkProvider);
    }

    /// <summary>
    /// Registers the activity-narration dispatcher. Register your <c>IAuditNarrator</c>
    /// implementations separately; they are discovered via DI.
    /// </summary>
    public static IServiceCollection AddKitStackAuditNarration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IActivityNarrationDispatcher, ActivityNarrationDispatcher>();
        return services;
    }

    private static IServiceCollection ConfigureAudit(
        IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? efSinkProvider)
    {
        var section = configuration.GetSection(AuditOptions.SectionName);
        services.Configure<AuditOptions>(section);

        var options = section.Get<AuditOptions>() ?? new AuditOptions();
        var sink = (options.Sink ?? "fake").Trim().ToLowerInvariant();

        switch (sink)
        {
            case "efcore":
            case "ef":
            case "sql":
                if (efSinkProvider is null)
                    throw new InvalidOperationException(
                        "Audit:Sink is 'efcore' but no EF provider delegate was supplied. " +
                        "Pass efSinkProvider to AddKitStackAudit, e.g. o => o.UseSqlServer(connectionString).");
                services.AddKitStackAuditEntityFrameworkCoreSink(efSinkProvider);
                break;

            case "mongo":
            case "mongodb":
                var db = options.Database ?? new AuditDatabaseOptions();
                if (string.IsNullOrWhiteSpace(db.ConnectionString) || string.IsNullOrWhiteSpace(db.DatabaseName))
                    throw new InvalidOperationException(
                        "Audit:Sink is 'mongo' but Audit:Database:ConnectionString / DatabaseName are not configured.");
                services.AddKitStackAuditMongoSink(db.ConnectionString!, db.DatabaseName!, db.CollectionName ?? "AuditTrails");
                break;

            case "fake":
            case "inmemory":
            case "memory":
                services.AddKitStackAuditInMemorySink();
                break;

            default:
                throw new InvalidOperationException(
                    $"Unknown Audit:Sink value '{options.Sink}'. Expected 'efcore', 'mongo', or 'fake'.");
        }

        return services;
    }
}
