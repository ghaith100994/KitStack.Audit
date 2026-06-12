namespace KitStack.Audit.Abstractions.Attributes;

/// <summary>
/// Marks a foreign-key property as the link to the owning/parent aggregate, so changes to the
/// child are also attributed to the parent via <c>RelatedEntityType</c>/<c>RelatedEntityId</c>
/// on the captured trail and activity.
/// <para>
/// Example: adding a <c>CommitteeMember</c> normally only produces a trail for the member row.
/// Annotating its FK with <c>[AuditParent("Committee")]</c> stamps the member's trails and
/// activities with the committee's id, so the committee's profile feed can show them too:
/// </para>
/// <code>
/// public class CommitteeMember
/// {
///     [AuditParent(nameof(Committee))]
///     public Guid CommitteeId { get; set; }
/// }
/// </code>
/// Feeds then query: <c>EntityType == "Committee" &amp;&amp; EntityId == id</c> OR
/// <c>RelatedEntityType == "Committee" &amp;&amp; RelatedEntityId == id</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class AuditParentAttribute : Attribute
{
    public AuditParentAttribute(string parentEntityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parentEntityType);
        ParentEntityType = parentEntityType;
    }

    /// <summary>Logical name of the parent entity (e.g. "Committee").</summary>
    public string ParentEntityType { get; }
}
