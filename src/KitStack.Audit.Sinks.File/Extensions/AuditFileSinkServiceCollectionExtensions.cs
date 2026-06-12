using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Sinks.File.Options;
using KitStack.Audit.Sinks.File.Sinks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KitStack.Audit.Sinks.File.Extensions;

/// <summary>
/// Registers the rolling JSON-Lines file sink. The same singleton instance backs both
/// <see cref="IAuditSink"/> and <see cref="IActivitySink"/>.
/// </summary>
public static class AuditFileSinkServiceCollectionExtensions
{
    public static IServiceCollection AddKitStackAuditFileSink(
        this IServiceCollection services,
        Action<FileAuditSinkOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<FileAuditSinkOptions>();
        if (configure is not null)
            services.Configure(configure);

        services.TryAddSingleton<FileAuditSink>();
        services.TryAddSingleton<IAuditSink>(sp => sp.GetRequiredService<FileAuditSink>());
        services.TryAddSingleton<IActivitySink>(sp => sp.GetRequiredService<FileAuditSink>());

        return services;
    }

    /// <summary>Registers the file sink writing into <paramref name="directory"/>.</summary>
    public static IServiceCollection AddKitStackAuditFileSink(
        this IServiceCollection services,
        string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        return services.AddKitStackAuditFileSink(o => o.Directory = directory);
    }
}
