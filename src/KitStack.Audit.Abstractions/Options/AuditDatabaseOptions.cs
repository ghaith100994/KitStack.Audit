namespace KitStack.Audit.Abstractions.Options;

/// <summary>
/// Connection details for database-backed sinks. Bound from "Audit:Database".
/// </summary>
public class AuditDatabaseOptions
{
    /// <summary>Relational provider hint: "sqlserver", "postgres", "mysql", "sqlite".</summary>
    public string? Provider { get; set; }

    public string? ConnectionString { get; set; }

    /// <summary>Database name (used by document stores such as Mongo).</summary>
    public string? DatabaseName { get; set; }

    /// <summary>Collection/table name override (used by document stores such as Mongo).</summary>
    public string? CollectionName { get; set; }
}
