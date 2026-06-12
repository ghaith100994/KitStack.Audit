using KitStack.Audit.Abstractions.Attributes;

namespace KitStack.Audit.Tests.Support;

[AuditDefinition("Governance", "Committees", "Committee", "Committee", "لجنة")]
public class Committee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

[AuditDefinition("Governance", "Committees", "CommitteeMember", "Committee member", "عضو لجنة")]
public class CommitteeMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Changes to this member are also attributed to the owning committee.</summary>
    [AuditParent(nameof(Committee))]
    public Guid CommitteeId { get; set; }

    public string MemberName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
