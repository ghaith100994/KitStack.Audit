namespace KitStack.Audit.Narration.Models;

/// <summary>A piece of text in English and Arabic, mapped onto the <c>*En</c>/<c>*Ar</c> fields of an activity event.</summary>
public readonly record struct LocalizedText(string En, string Ar)
{
    /// <summary>The same text in both languages (raw values, numbers, codes).</summary>
    public static LocalizedText Same(string text) => new(text, text);

    public override string ToString() => En;
}
