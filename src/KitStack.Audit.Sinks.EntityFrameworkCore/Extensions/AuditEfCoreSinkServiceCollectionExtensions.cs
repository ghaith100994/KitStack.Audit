using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Sinks.EntityFrameworkCore.Persistence;
using KitStack.Audit.Sinks.EntityFrameworkCore.Sinks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KitStack.Audit.Sinks.EntityFrameworkCore.Extensions;

/// <summary>
/// Registers the relational audit sink. The provider (SQL Server, PostgreSQL, ...) is supplied by
/// the caller so this package stays provider-agnostic.
///
/// <code>
/// services.AddKitStackAuditEntityFrameworkCoreSink(o =>
///     o.UseSqlServer(configuration.GetConnectionString("Audit")));
/// </code>
/// </summary>
public static class AuditEfCoreSinkServiceCollectionExtensions
{
    public static IServiceCollection AddKitStackAuditEntityFrameworkCoreSink(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddDbContext<AuditDbContext>(configureDbContext);
        services.TryAddScoped<IAuditSink, EfCoreAuditSink>();

        return services;
    }

    /// <summary>
    /// Overload for when provider configuration needs the service provider
    /// (e.g. to read connection details from bound options).
    /// </summary>
    public static IServiceCollection AddKitStackAuditEntityFrameworkCoreSink(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddDbContext<AuditDbContext>(configureDbContext);
        services.TryAddScoped<IAuditSink, EfCoreAuditSink>();

        return services;
    }
}
