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

    public AuditDatabaseOptions Database { get; set; } = new();
}
