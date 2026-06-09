using KitStack.Audit.Abstractions.Utilities;
using Xunit;

namespace KitStack.Audit.Tests;

public class AuditValueConverterTests
{
    [Fact]
    public void Null_stays_null()
        => Assert.Null(AuditValueConverter.Normalize(null));

    [Fact]
    public void DateOnly_is_formatted_invariantly()
        => Assert.Equal("2024-01-15", AuditValueConverter.Normalize(new DateOnly(2024, 1, 15)));

    [Fact]
    public void Unspecified_datetime_becomes_utc()
    {
        var unspecified = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);
        var result = Assert.IsType<DateTime>(AuditValueConverter.Normalize(unspecified));
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void Primitive_passes_through()
        => Assert.Equal(42, AuditValueConverter.Normalize(42));
}
