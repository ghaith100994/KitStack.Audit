using KitStack.Audit.Narration.Options;

namespace KitStack.Audit.Narration.Contracts;

/// <summary>Everything a <c>NarratorBase&lt;TEntity&gt;</c> needs, bundled so narrators take one constructor argument.</summary>
public interface INarrationServices
{
    NarrationOptions Options { get; }
    INarrationPhrases Phrases { get; }
    IEnumLabelProvider EnumLabels { get; }
}
