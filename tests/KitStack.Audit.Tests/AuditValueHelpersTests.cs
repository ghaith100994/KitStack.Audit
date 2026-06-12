using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Extensions;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Abstractions.Utilities;
using Xunit;

namespace KitStack.Audit.Tests;

public class AuditValueHelpersTests
{
    [Fact]
    public void Enum_normalizes_to_its_name()
        => Assert.Equal("Delete", AuditValueConverter.Normalize(TrailType.Delete));

    [Fact]
    public void Byte_array_normalizes_to_base64()
        => Assert.Equal(Convert.ToBase64String(new byte[] { 1, 2, 3 }),
            AuditValueConverter.Normalize(new byte[] { 1, 2, 3 }));

    [Fact]
    public void DateTimeOffset_normalizes_to_utc_datetime()
    {
        var offset = new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.FromHours(3));
        var result = Assert.IsType<DateTime>(AuditValueConverter.Normalize(offset));
        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(9, result.Hour);
    }

    [Fact]
    public void TrailDto_value_helpers_read_typed_values()
    {
        var dto = new TrailDto { Type = TrailType.Update };
        dto.OldValues["Quantity"] = 1;
        dto.NewValues["Quantity"] = 5L;
        dto.NewValues["Kind"] = "Update";   // enums are stored by name
        dto.ModifiedProperties.Add("Quantity");

        Assert.True(dto.HasChanged("quantity"));
        Assert.False(dto.HasChanged("Name"));
        Assert.Equal(1, dto.GetOldValue<int>("Quantity"));
        Assert.Equal(5, dto.GetNewValue<int>("Quantity"));
        Assert.Equal(TrailType.Update, dto.GetNewValue<TrailType>("Kind"));
        Assert.Null(dto.GetNewValue<string>("Missing"));
    }

    [Fact]
    public void Narration_context_helpers_fall_back_from_new_to_old_values()
    {
        var context = new AuditNarrationContext
        {
            EntityType = typeof(object),
            TrailType = TrailType.Delete,
            OldValues = new Dictionary<string, object?> { ["Name"] = "Gone" },
        };

        Assert.Equal("Gone", context.GetValue<string>("Name"));
        Assert.Equal("Gone", context.GetOldValue<string>("Name"));
        Assert.Null(context.GetNewValue<string>("Name"));
    }
}
