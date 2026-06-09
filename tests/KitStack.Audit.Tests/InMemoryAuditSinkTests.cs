using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Fakes.Contracts;
using KitStack.Audit.Fakes.Sinks;
using Xunit;

namespace KitStack.Audit.Tests;

public class InMemoryAuditSinkTests
{
    [Fact]
    public async Task WriteAsync_accumulates_then_Clear_empties()
    {
        var sink = new InMemoryAuditSink();
        IFakeAuditStore store = sink;

        await sink.WriteAsync(new[] { new TrailDto { TableName = "A" } });
        await sink.WriteAsync(new[] { new TrailDto { TableName = "B" } });
        Assert.Equal(2, store.Trails.Count);

        store.Clear();
        Assert.Empty(store.Trails);
    }

    [Fact]
    public async Task WriteAsync_ignores_empty_batches()
    {
        var sink = new InMemoryAuditSink();
        await sink.WriteAsync(Array.Empty<TrailDto>());
        Assert.Empty(((IFakeAuditStore)sink).Trails);
    }
}
