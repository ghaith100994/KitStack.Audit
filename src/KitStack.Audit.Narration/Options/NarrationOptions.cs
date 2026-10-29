namespace KitStack.Audit.Narration.Options;

/// <summary>Settings shared by every narrator built on <c>NarratorBase&lt;TEntity&gt;</c>.</summary>
public sealed class NarrationOptions
{
    /// <summary>
    /// Bookkeeping properties ignored when deciding whether an update touched user data.
    /// An update that changes only these is not narrated by <c>Standard</c>.
    /// </summary>
    public ISet<string> SystemFields { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "CreatedBy", "CreatedByName", "CreatedOn",
        "LastModifiedBy", "LastModifiedByName", "LastModifiedOn",
        "DeletedBy", "DeletedByName", "DeletedOn",
        "ActivationChangedBy", "ActivationChangedByName", "ActivationChangedOn",
    };

    /// <summary>
    /// Properties tried in order to name the entity in titles; the primary key is the fallback.
    /// A narrator base can override <c>NameProperties</c> to use its own list.
    /// </summary>
    public IList<string> NameProperties { get; } = new List<string>
    {
        "Number", "NameAr", "NameEn", "Title", "Name", "Code",
    };

    /// <summary>Soft-delete timestamp: set means deleted, cleared means restored.</summary>
    public string SoftDeleteProperty { get; set; } = "DeletedOn";

    /// <summary>Boolean property toggled by activation/deactivation.</summary>
    public string ActivationProperty { get; set; } = "IsActive";

    /// <summary>Icons used by the standard patterns.</summary>
    public NarrationIcons Icons { get; } = new();
}

public sealed class NarrationIcons
{
    public string Removed { get; set; } = "trash";
    public string Restored { get; set; } = "undo-2";
    public string Activated { get; set; } = "toggle-right";
    public string Deactivated { get; set; } = "toggle-left";
}
