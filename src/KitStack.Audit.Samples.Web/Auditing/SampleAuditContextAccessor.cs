using KitStack.Audit.Abstractions.Contracts;

namespace KitStack.Audit.Samples.Web.Auditing;

/// <summary>
/// Demo accessor with a fixed user. In a real app, derive this from the request principal
/// (e.g. inject IHttpContextAccessor and read the user's id/name from claims).
/// </summary>
public sealed class SampleAuditContextAccessor : IAuditContextAccessor
{
    public DefaultIdType UserId => Guid.Parse("00000000-0000-0000-0000-000000000001");
    public string? UserName => "sample-user";
}
