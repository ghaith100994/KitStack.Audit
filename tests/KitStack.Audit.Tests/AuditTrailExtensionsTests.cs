using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Extensions;
using KitStack.Audit.Abstractions.Models;
using Xunit;

namespace KitStack.Audit.Tests;

public class AuditTrailExtensionsTests
{
    [Fact]
    public void Persisted_trail_round_trips_through_the_read_helpers()
    {
        var dto = new TrailDto { TableName = "Order", Type = TrailType.Update };
        dto.KeyValues["Id"] = 7;
        dto.OldValues["Status"] = "Draft";
        dto.NewValues["Status"] = "Submitted";
        dto.NewValues["Total"] = 19.5;
        dto.ModifiedProperties.Add("Status");
        dto.ModifiedProperties.Add("Total");

        var trail = dto.ToAuditTrail();

        Assert.Equal(TrailType.Update, trail.GetTrailType());
        Assert.Equal(7, trail.GetPrimaryKey()["Id"].GetInt32());
        Assert.Equal("Draft", trail.GetPreviousValues()["Status"].GetString());
        Assert.Equal("Submitted", trail.GetNewValues()["Status"].GetString());
        Assert.Equal(19.5, trail.GetNewValues()["Total"].GetDouble());
        Assert.Equal(new[] { "Status", "Total" }, trail.GetModifiedProperties());
    }

    [Fact]
    public void Empty_payloads_return_empty_structures()
    {
        var trail = new AuditTrail { Operation = "bogus" };

        Assert.Null(trail.GetTrailType());
        Assert.Empty(trail.GetPreviousValues());
        Assert.Empty(trail.GetNewValues());
        Assert.Empty(trail.GetPrimaryKey());
        Assert.Empty(trail.GetModifiedProperties());
    }

    [Fact]
    public void ToAuditTrail_carries_the_new_context_fields()
    {
        var dto = new TrailDto
        {
            TableName = "Order",
            Type = TrailType.Create,
            UserName = "alice",
            TenantId = "tenant-1",
            CorrelationId = "corr-1",
            IpAddress = "10.0.0.1",
        };

        var trail = dto.ToAuditTrail();

        Assert.Equal("alice", trail.UserName);
        Assert.Equal("tenant-1", trail.TenantId);
        Assert.Equal("corr-1", trail.CorrelationId);
        Assert.Equal("10.0.0.1", trail.IpAddress);
    }
}
