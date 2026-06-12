namespace KitStack.Audit.Abstractions.Options;

/// <summary>
/// Top-level audit configuration. Bound from the "Audit" configuration section.
/// </summary>
public class AuditOptions
{
    public const string SectionName = "Audit";

    /// <summary>Selected sink: "efcore", "mongo", or "fake".</summary>
    public string Sink { get; set; } = "fake";

    /// <summary>
    /// When true, only entities marked auditable (attribute + registry) are recorded.
    /// When false, every aggregate root is audited.
    /// </summary>
    public bool EnforceAuditableSetting { get; set; } = true;

    /// <summary>Optional module name applied when a capture source does not provide one.</summary>
    public string? DefaultModule { get; set; }

    /// <summary>
    /// Property names that are never captured, in addition to properties marked with
    /// <c>[AuditIgnore]</c>. Matching is case-insensitive and applies to every entity.
    /// </summary>
    public IList<string> ExcludedProperties { get; } = new List<string>();

    /// <summary>
    /// Property names whose values are replaced by <see cref="MaskText"/> in audit payloads,
    /// in addition to properties marked with <c>[AuditMask]</c>. Matching is case-insensitive
    /// and applies to every entity. Use for global secrets like "Password" or "Token".
    /// </summary>
    public IList<string> MaskedProperties { get; } = new List<string>();

    /// <summary>Replacement text written for masked values.</summary>
    public string MaskText { get; set; } = "***";

    public AuditDatabaseOptions Database { get; set; } = new();
}
