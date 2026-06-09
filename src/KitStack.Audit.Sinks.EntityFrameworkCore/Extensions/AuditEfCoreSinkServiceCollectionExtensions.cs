using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Sinks.EntityFrameworkCore.Persistence;
using KitStack.Audit.Sinks.EntityFrameworkCore.Sinks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KitStack.Audit.Sinks.EntityFrameworkCore.Extensions;

/// <summary>
/// Registers the relational audit sink. The provider (SQL Server, PostgreSQL, ...) is supplied by
/// the caller so this package stays provider-agnostic. The same sink instance backs both
/// <see cref="IAuditSink"/> and <see cref="IActivitySink"/>.
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
        RegisterSink(services);
        return services;
    }

    public static IServiceCollection AddKitStackAuditEntityFrameworkCoreSink(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddDbContext<AuditDbContext>(configureDbContext);
        RegisterSink(services);
        return services;
    }

    private static void RegisterSink(IServiceCollection services)
    {
        services.TryAddScoped<EfCoreAuditSink>();
        services.TryAddScoped<IAuditSink>(sp => sp.GetRequiredService<EfCoreAuditSink>());
        services.TryAddScoped<IActivitySink>(sp => sp.GetRequiredService<EfCoreAuditSink>());
    }
}
