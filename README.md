# KitStack.Audit

[![Build](https://img.shields.io/badge/build-pending-lightgrey)](https://github.com/your-org/KitStack.Audit/actions)
[![License: Apache-2.0](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](./LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)

KitStack.Audit is a modular .NET library that provides audit-trail abstractions and pluggable implementations for capturing entity changes and persisting them to common backends (EF Core relational stores, MongoDB) plus an in-memory Fake sink for testing. It keeps abstractions small and dependency-free so application code stays decoupled from EF Core, Mongo, and messaging SDKs, and is designed to be easily extended with new capture sources and sinks.

## Why KitStack.Audit?

- Modular, well-scoped abstractions for audit trails and activity narration.
- Two independent extension points: **capture sources** (where changes are detected) and **sinks** (where trails are written), shipped as separate packages so consumers install only what they need.
- ASP.NET Core integration helpers for `IServiceCollection` and health-check registration.
- Fakes and samples that make testing and local development easy.
- Optional, opt-in auditing per table/aggregate via attributes and settings.
- Primary target: .NET 10.

## Features

- `IAuditSink` abstraction — write a batch of audit trails to any backend.
- `IAuditNarrator` / `IActivityNarrationDispatcher` — turn raw entity changes into human-readable activity events.
- EF Core `SaveChanges` interceptor that snapshots Added/Modified/Deleted aggregates and forwards them to the active sink.
- Opt-in auditing: gate per entity with `[AuditDefinition]` and an `IAuditableEntityRegistry` (e.g. an "auditable tables" setting).
- Sink implementations:
  - EF Core relational (SQL Server, PostgreSQL, ...)
  - MongoDB
  - Fake / in-memory sink for tests and dev
- ASP.NET Core DI helpers and health checks.
- Auditing never breaks the originating business transaction — sink failures are logged and swallowed.

## Sink & source status

- Capture sources
  - [x] EF Core `SaveChanges` interceptor (KitStack.Audit.EntityFrameworkCore)
  - [ ] Manual / explicit audit API
  - [ ] Message-bus consumer (community contributions welcome)
- Sinks
  - [x] EF Core relational (KitStack.Audit.Sinks.EntityFrameworkCore)
  - [x] MongoDB (KitStack.Audit.Sinks.Mongo)
  - [x] Fake / in-memory (KitStack.Audit.Fakes)
  - [ ] File / rolling-file sink
  - [ ] Serilog sink
  - [ ] Other stores / adapters (CosmosDB, Elasticsearch, etc.)
- Relational providers to standardize and validate:
  - [ ] SQL Server (MSSQL)
  - [ ] PostgreSQL
  - [ ] MySQL
  - [ ] SQLite (lightweight/local)

## How it works

Auditing has two independent axes:

1. **Capture** — *what changed and where it came from.* The EF Core interceptor inspects the `ChangeTracker` on save, builds a `TrailDto` per Added/Modified/Deleted aggregate, applies the auditable-table gate, and normalizes values for serialization.
2. **Sink** — *where the trail is written.* The captured trails are handed to the registered `IAuditSink` (EF Core, Mongo, or Fake). Sinks run detached from the originating `DbContext`, so a sink failure is logged and never rolls back the business transaction.

This separation means you can swap SQL Server for Mongo, or swap the EF interceptor for another capture source, without touching application code that depends only on `KitStack.Audit.Abstractions`.

## Database support (optional)

- Purpose: persist `AuditTrail` records and related metadata in a database alongside your application.
- Supported approaches (select and enable as needed):
  - EF Core (relational): SQL Server, PostgreSQL, MySQL, SQLite
  - Document stores: MongoDB
  - Custom stores/adapters (community/extension)
- Notes:
  - The audit store is typically a **separate** context/connection from the application database so auditing stays isolated.
  - Use environment variables, user secrets, or a secrets manager for connection strings and credentials.

## Installation

Install the abstractions plus the capture source and the sink you need:

```bash
dotnet add package KitStack.Audit.Abstractions
dotnet add package KitStack.Audit.EntityFrameworkCore
dotnet add package KitStack.Audit.Sinks.EntityFrameworkCore   # or KitStack.Audit.Sinks.Mongo
dotnet add package KitStack.Audit.AspNetCore
```

For tests:

```bash
dotnet add package KitStack.Audit.Fakes
```

## Configuration

Add an `Audit` section to your configuration:

```json
{
  "Audit": {
    "Sink": "efcore",
    "EnforceAuditableSetting": true,
    "Database": {
      "Provider": "sqlserver",
      "ConnectionString": "Server=...;Database=Audit;..."
    }
  }
}
```

- `Sink` — `efcore`, `mongo`, or `fake`.
- `EnforceAuditableSetting` — when `true`, only entities marked auditable (via `[AuditDefinition]` + registry) are recorded; when `false`, every aggregate root is audited.
- `Database` — connection/provider details for relational and document sinks.

## Usage

Register everything from the `Audit` section:

```csharp
builder.Services.AddKitStackAudit(builder.Configuration.GetSection("Audit"));
```

`AddKitStackAudit` binds `AuditOptions`, registers the EF Core interceptor, wires the sink selected by `Audit:Sink`, registers the narration dispatcher, and adds the corresponding health checks.

Mark an aggregate as auditable:

```csharp
[AuditDefinition(subModule: "Sales", resource: "Order", displayEn: "Order", displayAr: "طلب")]
public class Order : AggregateRoot { /* ... */ }
```

Provide a human-readable activity description by implementing a narrator:

```csharp
public sealed class OrderNarrator : AuditNarratorBase
{
    public override bool CanNarrate(Type entityType) => entityType == typeof(Order);

    public override ActivityEvent? Narrate(AuditNarrationContext ctx)
        => new ActivityEvent { /* map ctx to a friendly activity */ };
}
```

In tests, resolve the in-memory sink and assert what was captured:

```csharp
services.AddInMemoryFakeAudit();
// ... exercise a save ...
var store = provider.GetRequiredService<IFakeAuditStore>();
Assert.Single(store.Trails, t => t.TableName == nameof(Order));
```

## Repository layout

```
src/
  KitStack.Audit.Abstractions/             (IAuditSink, IAuditNarrator, AuditTrail, TrailDto, TrailType, AuditOptions, base classes)
  KitStack.Audit.EntityFrameworkCore/      (AuditSaveChangesInterceptor, [AuditDefinition], change capture)
  KitStack.Audit.Sinks.EntityFrameworkCore/(AuditDbContext, EfCoreAuditSink, relational health check)
  KitStack.Audit.Sinks.Mongo/              (MongoAuditSink, Bson serializer registration, health check)
  KitStack.Audit.Fakes/                    (InMemoryAuditSink, IFakeAuditStore)
  KitStack.Audit.AspNetCore/               (AddKitStackAudit, sink selection, BindSeparateDbContext, health-check registration)
  KitStack.Audit.Samples.Web/              (sample web app, DB-backed example)
tests/
  KitStack.Audit.Tests/                    (unit tests against the fake sink)
  KitStack.Audit.IntegrationTests/         (sink integration tests)
Directory.Build.props
Directory.Packages.props
.editorconfig
.gitignore
LICENSE
README.md
```

## Design notes

- Abstractions stay dependency-light: no EF Core, Mongo, MassTransit, or MediatR references in `KitStack.Audit.Abstractions`.
- The capture pipeline calls `IAuditSink.WriteAsync` directly rather than publishing a messaging/MediatR notification. Consumers who want a message bus can wrap a sink themselves.
- Ids are assigned by the capture layer, keeping `NewId`/MassTransit out of the abstractions.
- Value normalization (dates, `DateOnly`/`TimeOnly`, etc.) is centralized in a shared helper so every sink serializes consistently.

## Contributing

1. Fork and create a branch.
2. Implement features or fixes with tests.
3. Open a PR following repository guidelines.
4. Add documentation in `docs/` and update samples as needed.

## License

This project is licensed under the Apache-2.0 License — see the [LICENSE](./LICENSE) file for details.

## Maintainer

- ghaith100994 (initial author)