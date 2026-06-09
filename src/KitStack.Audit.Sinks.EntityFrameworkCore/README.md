# KitStack.Audit.EntityFrameworkCore

The EF Core **capture source** for KitStack.Audit. It provides a `SaveChanges` interceptor that
snapshots Added/Modified/Deleted entities into `TrailDto`s and forwards them to the registered
`IAuditSink`.

## How it behaves

- **Captures before the save** (original values are intact) and **writes after the save succeeds**,
  so a rolled-back transaction never leaves orphan trails.
- Only acts on `DbContext`s implementing `IAuditableDbContext` with `EnableAuditing == true`.
- Two-stage gate: an `IAuditEntityFilter` (default: everything; or `MarkerAuditEntityFilter<T>` for
  aggregate roots) followed by the optional `[AuditDefinition]` + `IAuditableEntityRegistry` check.
- Sink failures are logged and swallowed — auditing never breaks the business transaction.

## Wiring

```csharp
// 1. Mark your context
public class AppDbContext : DbContext, IAuditableDbContext
{
    public bool EnableAuditing => true;
    public string? AuditModule => "Sales";
}

// 2. Register the interceptor (restricting to aggregate roots in this example)
services.AddKitStackAuditCapture<IAggregateRoot>();

// 3. Attach it to the context
services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
});
```

`IAuditContextAccessor`, `IAuditSink`, and (optionally) `IAuditableEntityRegistry` must be
registered separately — typically by `KitStack.Audit.AspNetCore` or your application.

## Notes

- Use client-generated keys (e.g. GUIDs) to capture primary keys; store-generated keys are
  temporary at capture time and are skipped.
- For the synchronous `SaveChanges()` path the post-save write blocks on the async sink; prefer
  `SaveChangesAsync()`.

## License
Apache-2.0
