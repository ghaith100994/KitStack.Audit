using KitStack.Audit.Abstractions.Attributes;

namespace KitStack.Audit.Tests.Support;

/// <summary>Entity used by redaction tests: a masked secret, an ignored cache column, and a normal field.</summary>
public class SecureDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    [AuditMask]
    public string? Password { get; set; }

    [AuditMask("<hidden>")]
    public string? ApiKey { get; set; }

    [AuditIgnore]
    public string? SearchCache { get; set; }
}
