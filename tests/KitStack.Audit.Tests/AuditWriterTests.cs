using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Abstractions.Services;
using KitStack.Audit.Fakes.Sinks;
using Xunit;

namespace KitStack.Audit.Tests;

public class AuditWriterTests
{
    private readonly InMemoryAuditSink _sink = new();
    private readonly FakeAuditContextAccessor _accessor = new()
    {
        UserId = Guid.NewGuid(),
        UserName = "writer-user",
        TenantId = "tenant-9",
        CorrelationId = "corr-7",
    };

    private AuditWriter Writer => new(_sink, _accessor);

    [Fact]
    public async Task Record_stamps_user_context_onto_the_trail()
    {
        await Writer.RecordAsync(new TrailDto { TableName = "Job", Type = TrailType.Update });

        var trail = Assert.Single(_sink.Trails);
        Assert.Equal(_accessor.UserId, trail.UserId);
        Assert.Equal("writer-user", trail.UserName);
        Assert.Equal("tenant-9", trail.TenantId);
        Assert.Equal("corr-7", trail.CorrelationId);
    }

    [Fact]
    public async Task Record_does_not_overwrite_explicit_values()
    {
        var explicitUser = Guid.NewGuid();
        await Writer.RecordAsync(new TrailDto
        {
            TableName = "Job",
            Type = TrailType.Create,
            UserId = explicitUser,
            UserName = "someone-else",
        });

        var trail = Assert.Single(_sink.Trails);
        Assert.Equal(explicitUser, trail.UserId);
        Assert.Equal("someone-else", trail.UserName);
    }

    [Fact]
    public async Task Convenience_overload_builds_the_trail()
    {
        await Writer.RecordAsync(
            "ImportBatch",
            TrailType.Update,
            entityId: 42,
            oldValues: new Dictionary<string, object?> { ["Status"] = "Pending", ["Rows"] = 10 },
            newValues: new Dictionary<string, object?> { ["Status"] = "Done", ["Rows"] = 10 },
            module: "Imports");

        var trail = Assert.Single(_sink.Trails);
        Assert.Equal("ImportBatch", trail.TableName);
        Assert.Equal("Imports", trail.Module);
        Assert.Equal(42, trail.KeyValues["Id"]);
        Assert.Equal("Pending", trail.OldValues["Status"]);
        Assert.Equal("Done", trail.NewValues["Status"]);
        Assert.Equal(new[] { "Status" }, trail.ModifiedProperties);
    }

    [Fact]
    public async Task Empty_batch_is_a_no_op()
    {
        await Writer.RecordAsync(Array.Empty<TrailDto>());
        Assert.Empty(_sink.Trails);
    }
}
