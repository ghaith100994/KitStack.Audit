# KitStack.Audit.Sinks.Mongo

A MongoDB **sink** for KitStack.Audit, built on the official **MongoDB.EntityFrameworkCore**
provider. It persists captured trails through a `MongoAuditDbContext`, mirroring the structure of
the relational sink.

## Wiring

```csharp
services.AddKitStackAuditMongoSink(
    connectionString: configuration["Audit:Database:ConnectionString"]!,
    databaseName:     configuration["Audit:Database:DatabaseName"]!);
```

This registers `MongoAuditDbContext` (configured with `UseMongoDB`) and `IAuditSink -> MongoAuditSink`.
GUIDs are stored as strings via a globally registered serializer (registered idempotently).
Trails are written to the `AuditTrails` collection.

## Note on storage shape

Because EF Core cannot map arbitrary value dictionaries, each `TrailDto` is flattened to an
`AuditTrail` document whose value payloads (`PreviousValues`, `NewValues`, `ModifiedProperties`,
`PrimaryKey`) are JSON strings — the same shape as the relational sink. If you need native embedded
BSON sub-documents instead, use a raw `MongoDB.Driver`-based sink.

## Behavior

- Standalone MongoDB has no multi-document transactions, so the context uses
  `AutoTransactionBehavior.Never`.
- Failures are logged and swallowed so auditing never breaks the business transaction.

## License
Apache-2.0
