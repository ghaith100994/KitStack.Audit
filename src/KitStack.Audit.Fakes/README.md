# KitStack.Audit.Fakes

In-memory sink and test doubles for KitStack.Audit.

## What's here

- `InMemoryAuditSink` — an `IAuditSink` that records trails in memory, also exposed as
  `IFakeAuditStore` for assertions.
- `FakeAuditContextAccessor` — a settable `IAuditContextAccessor`.
- `AllowAllAuditableEntityRegistry` — an `IAuditableEntityRegistry` that audits everything.

## Usage

```csharp
services.AddKitStackAuditFakes(); // in-memory sink + fake accessor + allow-all registry

// ... run a save through the interceptor ...

var store = provider.GetRequiredService<IFakeAuditStore>();
Assert.Contains(store.Trails, t => t.TableName == nameof(Order));
```

Or register just the sink with `AddKitStackAuditInMemorySink()`.

## License
Apache-2.0
