using KitStack.Audit.Narration.Models;

namespace KitStack.Audit.Narration.Contracts;

/// <summary>
/// Resolves the display label of an enum value. Plug in your own to read labels from
/// attributes or resources (e.g. a <c>[DescriptionArEn]</c> attribute).
/// </summary>
public interface IEnumLabelProvider
{
    LocalizedText GetLabel(Enum value);
}
