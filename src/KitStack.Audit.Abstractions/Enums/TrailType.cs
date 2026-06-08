namespace KitStack.Audit.Abstractions.Enums;

/// <summary>
/// The kind of change captured by an audit trail.
/// </summary>
public enum TrailType
{
    None = 0,
    Create = 1,
    Update = 2,
    Delete = 3,
}
