using KitStack.Audit.Abstractions.Attributes;

namespace KitStack.Audit.Tests.Support;

/// <summary>Entity with two parents: changes are attributed to its organization AND its department.</summary>
[AuditDefinition("HR", "People", "Person", "Person", "شخص")]
public class Person
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [AuditParent("Organization")]
    public Guid OrganizationId { get; set; }

    [AuditParent("Department")]
    public Guid DepartmentId { get; set; }

    public string Name { get; set; } = string.Empty;
}
