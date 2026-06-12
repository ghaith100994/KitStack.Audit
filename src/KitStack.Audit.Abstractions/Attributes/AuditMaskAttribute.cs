namespace KitStack.Audit.Abstractions.Attributes;

/// <summary>
/// Masks a property's value in audit payloads. The change is still recorded —
/// the property shows up in the modified-property list — but the stored old/new
/// values are replaced by the mask text (<c>AuditOptions.MaskText</c> by default).
/// Use for secrets and PII such as passwords, tokens, and national IDs.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class AuditMaskAttribute : Attribute
{
    public AuditMaskAttribute(string? maskText = null) => MaskText = maskText;

    /// <summary>Optional per-property mask overriding <c>AuditOptions.MaskText</c>.</summary>
    public string? MaskText { get; }
}
