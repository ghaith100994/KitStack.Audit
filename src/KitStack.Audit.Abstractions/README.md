# KitStack.Audit.Abstractions

KitStack.Audit.Abstractions contains lightweight interfaces, models and utilities for the
KitStack.Audit capture sources and sinks.

This package is dependency-free (no EF Core, Mongo, MediatR or messaging SDKs) so that domain
and application code can depend on audit contracts (`IAuditSink`, `IAuditNarrator`,
`AuditTrail`, `TrailDto`) without pulling in provider packages.

For full documentation and examples, see the project repository:
https://github.com/ghaith100994/KitStack.Audit

## License
Apache-2.0
