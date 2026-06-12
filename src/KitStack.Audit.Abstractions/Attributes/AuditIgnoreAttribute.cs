namespace KitStack.Audit.Abstractions.Attributes;

/// <summary>
/// Excludes a property from audit capture entirely: it never appears in
/// old/new value payloads or in the modified-property list.
/// Use for noisy or irrelevant columns (row versions, computed caches, ...).
/// For sensitive values that should still be *recorded as changed* use
/// <see cref="AuditMaskAttribute"/> instead.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class AuditIgnoreAttribute : Attribute
{
}
