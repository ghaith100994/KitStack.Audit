using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Extensions;
using KitStack.Audit.Abstractions.Models;
using Xunit;

namespace KitStack.Audit.Tests;

public class TrailDtoExtensionsTests
{
    [Fact]
    public void ToAuditTrail_maps_core_fields_and_serializes_values()
    {
        var dto = new TrailDto
        {
            TableName = "Order",
            UserId = Guid.NewGuid(),
            Type = TrailType.Update,
            Module = "Sales",
        };
        dto.KeyValues["Id"] = Guid.NewGuid();
        dto.OldValues["Status"] = "Draft";
        dto.NewValues["Status"] = "Submitted";
        dto.ModifiedProperties.Add("Status");

        var trail = dto.ToAuditTrail();

        Assert.Equal("Order", trail.Entity);
        Assert.Equal("Update", trail.Operation);
        Assert.Equal("Sales", trail.Module);
        Assert.NotNull(trail.PrimaryKey);
        Assert.Contains("Submitted", trail.NewValues!);
        Assert.Contains("Draft", trail.PreviousValues!);
        Assert.Contains("Status", trail.ModifiedProperties!);
    }

    [Fact]
    public void ToAuditTrail_leaves_empty_collections_null()
    {
        var dto = new TrailDto { TableName = "X", Type = TrailType.Create };

        var trail = dto.ToAuditTrail();

        Assert.Null(trail.PreviousValues);
        Assert.Null(trail.NewValues);
        Assert.Null(trail.PrimaryKey);
        Assert.Null(trail.ModifiedProperties);
    }
}
