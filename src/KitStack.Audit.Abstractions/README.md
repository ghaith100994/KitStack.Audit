# KitStack.Audit.Abstractions

KitStack.Audit.Abstractions contains lightweight interfaces, models and utilities for the
KitStack.Audit capture sources and sinks.

This package is dependency-free (no EF Core, Mongo, MediatR or messaging SDKs) so that domain
and application code can depend on audit contracts (`IAuditSink`, `IAuditWriter`,
`IAuditNarrator`, `AuditTrail`, `TrailDto`) without pulling in provider packages.

It also ships:

- `[AuditIgnore]` / `[AuditMask]` attributes for redacting sensitive properties.
- `AuditWriter` — the manual capture source behind `IAuditWriter`.
- `CompositeAuditSink` — fans a single write out to several sinks.
- Typed read helpers: `GetOldValue<T>`/`GetNewValue<T>`/`HasChanged` on `TrailDto` and
  `AuditNarrationContext`, plus JSON round-trip helpers on persisted `AuditTrail` records.

For full documentation and examples, see the project repository:
https://github.com/ghaith100994/KitStack.Audit

## License
Apache-2.0
