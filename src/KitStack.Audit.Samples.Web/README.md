# KitStack.Audit.Samples.Web

A minimal ASP.NET Core app that wires KitStack.Audit end to end against SQLite (no external
infrastructure required).

## Run

```bash
dotnet run
```

Then:

```bash
# create -> Create trail
curl -s -X POST http://localhost:5000/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Widget","price":9.99}'

# update -> Update trail (changed properties only)
curl -s -X PUT http://localhost:5000/products/<id> \
  -H "Content-Type: application/json" \
  -d '{"name":"Widget v2","price":12.50,"isActive":true}'

# delete -> Delete trail
curl -s -X DELETE http://localhost:5000/products/<id>

# read the captured trails
curl -s http://localhost:5000/audit
```

## What it demonstrates

- `SampleDbContext` implements `IAuditableDbContext` (opts in, supplies the module).
- `Product` is an aggregate root with `[AuditDefinition]`.
- `AddKitStackAudit<IAggregateRoot>(...)` registers capture + the efcore sink.
- The interceptor is attached to the business context; the audit store is a separate SQLite db.

## License
Apache-2.0
