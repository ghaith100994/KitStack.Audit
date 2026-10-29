using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Narration.Enums;
using KitStack.Audit.Narration.Extensions;
using KitStack.Audit.Narration.Services;
using Xunit;

namespace KitStack.Audit.Tests.Narration;

public class NarratorHelperTests
{
    private readonly ProbeNarrator _probe = new(NarrationServices.CreateDefault());

    [Fact]
    public void Val_prefers_new_then_old()
    {
        var c = Trail.Of(TrailType.Update,
            oldValues: new() { ["A"] = "old", ["B"] = "oldB" },
            newValues: new() { ["A"] = "new", ["B"] = null });
        Assert.Equal("new", _probe.V(c, "A"));
        Assert.Equal("oldB", _probe.V(c, "B"));
        Assert.Null(_probe.V(c, "Missing"));
    }

    [Fact]
    public void Typed_val_converts_trail_values()
    {
        var c = Trail.Of(TrailType.Delete, oldValues: new() { ["Status"] = "Shipped", ["Qty"] = 3L });
        Assert.Equal(ShopOrderStatus.Shipped, _probe.V<ShopOrderStatus>(c, "Status"));
        Assert.Equal(3, _probe.V<int>(c, "Qty"));
    }

    [Fact]
    public void Enum_label_accepts_enum_name_or_number()
    {
        Assert.Equal("Shipped", _probe.Label<ShopOrderStatus>("Shipped").En);
        Assert.Equal("Shipped", _probe.Label<ShopOrderStatus>(1).Ar);
        Assert.Equal("—", _probe.Label<ShopOrderStatus>(null).En);
        Assert.Equal("Unknown", _probe.Label<ShopOrderStatus>("Unknown").En);
    }

    [Fact]
    public void Moved_and_WasIn_compare_old_and_new()
    {
        var c = Trail.Of(TrailType.Update,
            oldValues: new() { ["Status"] = "Pending" },
            newValues: new() { ["Status"] = "Shipped" },
            modified: ["Status"]);
        Assert.True(_probe.IsMoved(c));
        Assert.True(_probe.IsWasIn(c));
    }

    [Fact]
    public void Name_uses_configured_properties_in_order()
    {
        var probe = new ProbeNarrator(NarrationServices.CreateDefault(o =>
        {
            o.NameProperties.Clear();
            o.NameProperties.Add("Code");
        }));
        var c = Trail.Of(TrailType.Create, newValues: new() { ["Number"] = "SO-1", ["Code"] = "C-9" });
        Assert.Equal("C-9", probe.NameOf(c));
        Assert.Equal("SO-1", _probe.NameOf(c));
    }

    [Fact]
    public void System_fields_are_configurable()
    {
        var probe = new ProbeNarrator(NarrationServices.CreateDefault(o => o.SystemFields.Add("RowVersion")));
        var c = Trail.Of(TrailType.Update, modified: ["RowVersion", "Note", "createdon"]);
        Assert.Equal(["Note"], probe.Fields(c));
        Assert.Equal(["RowVersion", "Note"], _probe.Fields(c));
    }

    [Fact]
    public void Soft_delete_property_is_configurable()
    {
        var probe = new ProbeNarrator(NarrationServices.CreateDefault(o => o.SoftDeleteProperty = "ArchivedAt"));
        var archived = Trail.Of(TrailType.Update, newValues: new() { ["ArchivedAt"] = Trail.At }, modified: ["ArchivedAt"]);
        var unarchived = Trail.Of(TrailType.Update, newValues: new() { ["ArchivedAt"] = null }, modified: ["ArchivedAt"]);
        Assert.True(probe.IsSoftDeleted(archived));
        Assert.True(probe.IsRestored(unarchived));
        Assert.False(_probe.IsSoftDeleted(archived));
    }

    [Fact]
    public void Formatting_helpers()
    {
        Assert.Equal("2026-01-15 09:30", ProbeNarrator.Date(Trail.At));
        Assert.Equal("2026-01-15", ProbeNarrator.Date(Trail.At.Date));
        Assert.Equal("2026-01-15", ProbeNarrator.Date("2026-01-15"));
        Assert.Null(ProbeNarrator.Date(null));
        Assert.Equal("abc…", ProbeNarrator.Cut("abc def", 4));
        Assert.Equal("abc", ProbeNarrator.Cut("abc", 4));
        Assert.Equal("a — b", ProbeNarrator.Joined("a", null, " ", "b"));
        Assert.Null(ProbeNarrator.Joined(null, ""));
    }

    [Theory]
    [InlineData(Severity.Info, "info")]
    [InlineData(Severity.Success, "success")]
    [InlineData(Severity.Warning, "warning")]
    [InlineData(Severity.Critical, "critical")]
    public void Severity_round_trips_through_its_wire_value(Severity severity, string wire)
    {
        Assert.Equal(wire, severity.ToWireValue());
        Assert.Equal(severity, SeverityExtensions.ParseSeverity(wire));
    }
}
