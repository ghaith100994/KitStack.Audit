using KitStack.Audit.EntityFrameworkCore.Contracts;
using KitStack.Audit.EntityFrameworkCore.Filtering;
using KitStack.Audit.EntityFrameworkCore.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KitStack.Audit.EntityFrameworkCore.Extensions;

/// <summary>
/// Registration helpers for the EF Core capture source.
///
/// These register the interceptor and a default entity filter only. The interceptor also
/// depends on <c>IAuditContextAccessor</c> and <c>IAuditSink</c> (and optionally
/// <c>IAuditableEntityRegistry</c>) which must be registered by the application or the
/// KitStack.Audit.AspNetCore orchestrator.
///
/// Attach the interceptor to each audited context:
/// <code>
/// services.AddDbContext&lt;AppDbContext&gt;((sp, o) =>
/// {
///     o.UseSqlServer(connectionString);
///     o.AddInterceptors(sp.GetRequiredService&lt;AuditSaveChangesInterceptor&gt;());
/// });
/// </code>
/// </summary>
public static class AuditEntityFrameworkCoreServiceCollectionExtensions
{
    /// <summary>Register the interceptor with the default (audit-everything) entity filter.</summary>
    public static IServiceCollection AddKitStackAuditCapture(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IAuditEntityFilter, DefaultAuditEntityFilter>();
        services.TryAddScoped<AuditSaveChangesInterceptor>();
        return services;
    }

    /// <summary>
    /// Register the interceptor restricting auditing to entities assignable to
    /// <typeparamref name="TMarker"/> (e.g. an aggregate-root marker interface).
    /// </summary>
    public static IServiceCollection AddKitStackAuditCapture<TMarker>(this IServiceCollection services)
        where TMarker : class
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IAuditEntityFilter, MarkerAuditEntityFilter<TMarker>>();
        services.TryAddScoped<AuditSaveChangesInterceptor>();
        return services;
    }
}
