using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using KitStack.Audit.Narration.Contracts;
using KitStack.Audit.Narration.Models;

namespace KitStack.Audit.Narration.Services;

/// <summary>
/// Labels an enum value with its <see cref="DescriptionAttribute"/> text, or its name,
/// in both languages. Replace it to supply real Arabic labels.
/// </summary>
public sealed class DefaultEnumLabelProvider : IEnumLabelProvider
{
    private readonly ConcurrentDictionary<Enum, LocalizedText> _cache = new();

    public LocalizedText GetLabel(Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return _cache.GetOrAdd(value, static v =>
        {
            var name = v.ToString();
            var description = v.GetType().GetField(name)?.GetCustomAttribute<DescriptionAttribute>()?.Description;
            return LocalizedText.Same(description ?? name);
        });
    }
}
