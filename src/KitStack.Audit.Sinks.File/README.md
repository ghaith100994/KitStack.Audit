# KitStack.Audit.Sinks.File

Rolling JSON-Lines file sink for [KitStack.Audit](https://github.com/ghaith100994/KitStack.Audit).
Appends each audit trail / activity event as one JSON document per line to `.jsonl` files,
rolling by UTC day (default), month, or never.

Useful for lightweight deployments without a database, shipping audit data into log pipelines
(Filebeat, Fluent Bit, Vector, ...), and local diagnostics.

## Install

```bash
dotnet add package KitStack.Audit.Sinks.File
```

## Usage

Directly:

```csharp
services.AddKitStackAuditFileSink(o =>
{
    o.Directory = "/var/log/myapp/audit";
    o.RollInterval = FileRollInterval.Day;   // None | Day | Month
});
```

Or via the ASP.NET Core orchestrator and configuration:

```json
{
  "Audit": {
    "Sink": "file",
    "File": {
      "Directory": "audit-logs",
      "RollInterval": "Day"
    }
  }
}
```

```csharp
builder.Services.AddKitStackAudit(builder.Configuration.GetSection("Audit"));
```

## Output

`audit-logs/audit-20260612.jsonl`:

```json
{"Id":"...","UserId":"...","TableName":"Order","Type":1,"NewValues":{"Status":"Paid"},...}
```

Trails and activity events are written to separate files (`audit-*.jsonl` / `activity-*.jsonl`).
Writes are append-only and serialized through a single gate, and failures are logged and
swallowed so the originating business transaction is never affected.
