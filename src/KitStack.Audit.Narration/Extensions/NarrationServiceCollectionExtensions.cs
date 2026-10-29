using System.Reflection;
using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Narration.Contracts;
using KitStack.Audit.Narration.Options;
using KitStack.Audit.Narration.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KitStack.Audit.Narration.Extensions;

public static class NarrationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the services <see cref="NarratorBase{TEntity}"/> depends on: <see cref="NarrationOptions"/>,
    /// <see cref="INarrationPhrases"/>, <see cref="IEnumLabelProvider"/> and <see cref="INarrationServices"/>.
    /// Safe to call more than once; every <paramref name="configure"/> applies to the same options.
    /// The narration dispatcher itself is registered by <c>AddKitStackAuditNarration</c> (KitStack.Audit.AspNetCore).
    /// </summary>
    public static IServiceCollection AddKitStackAuditNarrationToolkit(
        this IServiceCollection services,
        Action<NarrationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(NarrationOptions));
        NarrationOptions options;
        if (descriptor is null)
        {
            options = new NarrationOptions();
            services.AddSingleton(options);
        }
        else
        {
            options = descriptor.ImplementationInstance as NarrationOptions
                ?? throw new InvalidOperationException(
                    $"{nameof(NarrationOptions)} is already registered without an instance; register it through {nameof(AddKitStackAuditNarrationToolkit)} only.");
        }

        configure?.Invoke(options);

        services.TryAddSingleton<INarrationPhrases, DefaultNarrationPhrases>();
        services.TryAddSingleton<IEnumLabelProvider, DefaultEnumLabelProvider>();
        services.TryAddSingleton<INarrationServices, NarrationServices>();
        return services;
    }

    /// <summary>Replaces the enum label source (e.g. one that reads a bilingual attribute).</summary>
    public static IServiceCollection AddNarrationEnumLabelProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IEnumLabelProvider
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.Replace(ServiceDescriptor.Singleton<IEnumLabelProvider, TProvider>());
    }

    /// <summary>Replaces the sentence templates used by the standard patterns.</summary>
    public static IServiceCollection AddNarrationPhrases<TPhrases>(this IServiceCollection services)
        where TPhrases : class, INarrationPhrases
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.Replace(ServiceDescriptor.Singleton<INarrationPhrases, TPhrases>());
    }

    /// <summary>
    /// Registers every concrete <see cref="IAuditNarrator"/> in <paramref name="assembly"/> as a singleton.
    /// When <paramref name="baseNarratorType"/> is given (closed, or an open generic such as
    /// <c>typeof(MyModuleNarrator&lt;&gt;)</c>), only narrators deriving from it are registered.
    /// A narrator already registered is not added twice.
    /// </summary>
    public static IServiceCollection AddAuditNarratorsFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        Type? baseNarratorType = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        var narrators = LoadableTypes(assembly)
            .Where(t => t is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false }
                        && typeof(IAuditNarrator).IsAssignableFrom(t)
                        && (baseNarratorType is null || DerivesFrom(t, baseNarratorType)));

        foreach (var narrator in narrators)
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IAuditNarrator), narrator));

        return services;
    }

    private static bool DerivesFrom(Type type, Type baseType)
    {
        if (!baseType.IsGenericTypeDefinition)
            return baseType.IsAssignableFrom(type);

        if (baseType.IsInterface)
            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == baseType);

        for (var t = type.BaseType; t is not null; t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == baseType)
                return true;
        }

        return false;
    }

    private static IEnumerable<Type> LoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.OfType<Type>();
        }
    }
}
