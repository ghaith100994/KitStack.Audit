namespace KitStack.Audit.Abstractions.Attributes;

/// <summary>
/// Marks an aggregate/entity as auditable and describes it.
/// The capture pipeline reads <see cref="Resource"/> to decide whether the
/// entity should be recorded (in combination with <c>IAuditableEntityRegistry</c>).
/// Derive from this attribute to create module-specific definitions.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AuditDefinitionAttribute : Attribute
{
    public AuditDefinitionAttribute(
        string module,
        string subModule,
        string resource,
        string displayEn,
        string displayAr)
    {
        Module = module;
        SubModule = subModule;
        Resource = resource;
        DisplayEn = displayEn;
        DisplayAr = displayAr;
    }

    /// <summary>Top-level module the entity belongs to (e.g. "Sales").</summary>
    public string Module { get; }

    /// <summary>Sub-module / feature area.</summary>
    public string SubModule { get; }

    /// <summary>Logical resource name used by the auditable-entity gate.</summary>
    public string Resource { get; }

    /// <summary>English display name.</summary>
    public string DisplayEn { get; }

    /// <summary>Arabic display name.</summary>
    public string DisplayAr { get; }
}
