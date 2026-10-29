using KitStack.Audit.Narration.Models;

namespace KitStack.Audit.Narration.Contracts;

/// <summary>
/// Sentence templates used by the standard narration patterns. Narrator output is persisted,
/// so changing a phrase changes how new history reads next to old history.
/// </summary>
public interface INarrationPhrases
{
    /// <summary>Placeholder for a missing value (e.g. a null enum).</summary>
    string EmptyValue { get; }

    LocalizedText Created(LocalizedText label, string name);
    LocalizedText Updated(LocalizedText label, string name);
    LocalizedText Removed(LocalizedText label, string name);
    LocalizedText Restored(LocalizedText label, string name);
    LocalizedText Activated(LocalizedText label, string name);
    LocalizedText Deactivated(LocalizedText label, string name);

    /// <summary>"{label} changed: {name}", used for a single tracked property (e.g. a status).</summary>
    LocalizedText Changed(LocalizedText label, string name);

    /// <summary>Subtitle of an update: how many user fields changed.</summary>
    LocalizedText FieldsChanged(int count);

    /// <summary>Subtitle of an activation.</summary>
    LocalizedText StatusActive { get; }

    /// <summary>Subtitle of a deactivation.</summary>
    LocalizedText StatusInactive { get; }

    /// <summary>"old → new" subtitle of a value change.</summary>
    LocalizedText Transition(LocalizedText from, LocalizedText to);
}
