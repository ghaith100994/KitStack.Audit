# KitStack.Audit.AspNetCore

One-call wiring for KitStack.Audit. Binds `AuditOptions`, registers the EF Core capture source
and the manual `IAuditWriter`, and selects a sink from configuration.

## Setup

```json
{
  "Audit": {
    "Sink": "efcore",
    "EnforceAuditableSetting": true,
    "Database": {
      "ConnectionString": "Server=...;Database=Audit;...",
      "DatabaseName": "Audit",
      "CollectionName": "AuditTrails"
    }
  }
}
```

```csharp
// efcore sink: supply your relational provider (driver lives in your app)
builder.Services.AddKitStackAudit<IAggregateRoot>(
    builder.Configuration,
    efSinkProvider: o => o.UseSqlServer(builder.Configuration.GetConnectionString("Audit")));

// mongo / file / fake sinks need no provider delegate:
// builder.Services.AddKitStackAudit(builder.Configuration);

// or configure everything in code (no IConfiguration section):
// builder.Services.AddKitStackAudit(o => { o.Sink = "file"; o.MaskedProperties.Add("Password"); });

// the current user (application-specific) and interceptor attachment are your responsibility:
builder.Services.AddScoped<IAuditContextAccessor, CurrentUserAuditContextAccessor>();

builder.Services.AddDbContext<AppDbContext>((sp, o) =>
{
    o.UseSqlServer(connectionString);
    o.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
});

// optional
builder.Services.AddKitStackAuditNarration();
builder.Services.AddHealthChecks().AddKitStackAudit();
```

## Sink selection (`Audit:Sink`)

| Value     | Sink                                          | Needs                                            |
|-----------|-----------------------------------------------|--------------------------------------------------|
| `efcore`  | relational (EF Core)                          | `efSinkProvider` delegate (your DB driver)       |
| `mongo`   | MongoDB                                       | `Audit:Database:ConnectionString` + `DatabaseName` |
| `file`    | JSON Lines (`KitStack.Audit.Sinks.File`)      | optional `Audit:File` section (directory, rolling) |
| `fake`    | in-memory (`KitStack.Audit.Fakes`)            | nothing                                          |

## License
Apache-2.0
