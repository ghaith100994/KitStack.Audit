# KitStack.Audit.Sinks.EntityFrameworkCore

A relational **sink** for KitStack.Audit. It persists captured `TrailDto`s as flat `AuditTrail`
rows via its own `AuditDbContext`, on any EF Core relational provider.

This package is provider-agnostic — it references EF Core but not a specific driver. You supply
`UseSqlServer` / `UseNpgsql` / `UseSqlite` etc. at registration.

## Wiring

```csharp
services.AddKitStackAuditEntityFrameworkCoreSink(o =>
    o.UseSqlServer(configuration.GetConnectionString("Audit")));
```

This registers `AuditDbContext` and `IAuditSink -> EfCoreAuditSink`. Combine with
`KitStack.Audit.EntityFrameworkCore` (the capture source) so the interceptor has a sink to write to.

## Schema

- Table `AuditTrails`, keyed on `Id`.
- `Operation`, `Entity`, `Module` are bounded strings.
- `PreviousValues`, `NewValues`, `ModifiedProperties`, `PrimaryKey` hold JSON and use the provider's
  large-text type.
- Indexes on `DateTime`, `Entity`, and `UserId`.

Create the schema with an EF Core migration against `AuditDbContext` (or `EnsureCreated()` in
development). The audit store is intentionally separate from your application database.

## Behavior

- Inserts the batch, swallows and logs any failure (auditing never breaks the business transaction).
- `AuditDbContext` does not implement `IAuditableDbContext`, so the audit store is never itself audited.

## License
Apache-2.0
