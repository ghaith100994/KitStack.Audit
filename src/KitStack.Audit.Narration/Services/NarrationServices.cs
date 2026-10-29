using KitStack.Audit.Narration.Contracts;
using KitStack.Audit.Narration.Options;

namespace KitStack.Audit.Narration.Services;

public sealed class NarrationServices : INarrationServices
{
    public NarrationServices(NarrationOptions options, INarrationPhrases phrases, IEnumLabelProvider enumLabels)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(phrases);
        ArgumentNullException.ThrowIfNull(enumLabels);
        Options = options;
        Phrases = phrases;
        EnumLabels = enumLabels;
    }

    public NarrationOptions Options { get; }
    public INarrationPhrases Phrases { get; }
    public IEnumLabelProvider EnumLabels { get; }

    /// <summary>Default services without a container (tests, console tools).</summary>
    public static NarrationServices CreateDefault(
        Action<NarrationOptions>? configure = null,
        INarrationPhrases? phrases = null,
        IEnumLabelProvider? enumLabels = null)
    {
        var options = new NarrationOptions();
        configure?.Invoke(options);
        return new NarrationServices(options, phrases ?? new DefaultNarrationPhrases(), enumLabels ?? new DefaultEnumLabelProvider());
    }
}
