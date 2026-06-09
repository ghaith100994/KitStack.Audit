# KitStack.Audit.IntegrationTests

End-to-end tests for the capture interceptor writing through the relational sink
(`EfCoreAuditSink` -> `AuditDbContext`) on SQLite. No external infrastructure required.

Covered:
- a committed save persists a correctly mapped `AuditTrail`
- a failed save (duplicate key) leaves no audit trail (the SavedChanges/SaveChangesFailed split)

A MongoDB sink integration test is intentionally omitted here because it needs a running server;
add one with Testcontainers or a `[Fact(Skip=...)]` guarded by a connection-string env var.
