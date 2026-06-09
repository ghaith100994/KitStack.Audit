using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Fakes.Contracts;
using KitStack.Audit.Fakes.Sinks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KitStack.Audit.Fakes.Extensions;

/// <summary>
/// Registration helpers for the in-memory sink and test doubles.
/// </summary>
public static class AuditFakesServiceCollectionExtensions
{
    /// <summary>
    /// Registers a single <see cref="InMemoryAuditSink"/> exposed as
    /// <see cref="IAuditSink"/>, <see cref="IActivitySink"/>, and <see cref="IFakeAuditStore"/>.
    /// </summary>
    public static IServiceCollection AddKitStackAuditInMemorySink(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<InMemoryAuditSink>();
        services.TryAddSingleton<IAuditSink>(sp => sp.GetRequiredService<InMemoryAuditSink>());
        services.TryAddSingleton<IActivitySink>(sp => sp.GetRequiredService<InMemoryAuditSink>());
        services.TryAddSingleton<IFakeAuditStore>(sp => sp.GetRequiredService<InMemoryAuditSink>());

        return services;
    }

    /// <summary>
    /// Registers fake test doubles: the in-memory sink, an allow-all registry, and a context accessor.
    /// </summary>
    public static IServiceCollection AddKitStackAuditFakes(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddKitStackAuditInMemorySink();
        services.TryAddSingleton<IAuditContextAccessor, FakeAuditContextAccessor>();
        services.TryAddSingleton<IAuditableEntityRegistry, AllowAllAuditableEntityRegistry>();

        return services;
    }
}
