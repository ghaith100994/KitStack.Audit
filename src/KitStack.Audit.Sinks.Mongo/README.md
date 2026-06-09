# KitStack.Audit.Sinks.Mongo

A MongoDB **sink** for KitStack.Audit. It persists captured `TrailDto`s as native documents,
keeping key/old/new values as embedded documents rather than flattening them to JSON strings.

## Wiring

```csharp
services.AddKitStackAuditMongoSink(
    connectionString: configuration["Audit:Database:ConnectionString"]!,
    databaseName:     configuration["Audit:Database:DatabaseName"]!,
    collectionName:   "AuditTrails");
```

This registers an `IMongoClient`, the collection, and `IAuditSink -> MongoAuditSink`. GUIDs are
stored as strings via a globally registered serializer (registered idempotently).

## Behavior

- Inserts the batch with `InsertManyAsync`; swallows and logs any failure so auditing never
  breaks the business transaction.

## License
Apache-2.0
