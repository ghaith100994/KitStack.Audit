using KitStack.Audit.Abstractions.Contracts;

namespace KitStack.Audit.Fakes.Sinks;

/// <summary>
/// A settable <see cref="IAuditContextAccessor"/> for tests.
/// </summary>
public sealed class FakeAuditContextAccessor : IAuditContextAccessor
{
    public DefaultIdType UserId { get; set; }
    public string? UserName { get; set; } = "test-user";
}
