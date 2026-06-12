namespace KitStack.Audit.Sinks.File.Options;

/// <summary>How often the file sink starts a new file.</summary>
public enum FileRollInterval
{
    /// <summary>Single file, never rolls (audit.jsonl).</summary>
    None,

    /// <summary>One file per UTC day (audit-20260612.jsonl).</summary>
    Day,

    /// <summary>One file per UTC month (audit-202606.jsonl).</summary>
    Month,
}

/// <summary>
/// Settings for the JSON-Lines file sink. Bound from "Audit:File" when using the
/// ASP.NET Core orchestrator, or configured in code via <c>AddKitStackAuditFileSink</c>.
/// </summary>
public class FileAuditSinkOptions
{
    /// <summary>Directory the log files are written to. Created on first write. Relative paths resolve against the working directory.</summary>
    public string Directory { get; set; } = "audit-logs";

    /// <summary>File-name prefix for trail files.</summary>
    public string TrailFilePrefix { get; set; } = "audit";

    /// <summary>File-name prefix for activity-event files.</summary>
    public string ActivityFilePrefix { get; set; } = "activity";

    /// <summary>Rolling policy; defaults to one file per UTC day.</summary>
    public FileRollInterval RollInterval { get; set; } = FileRollInterval.Day;
}
