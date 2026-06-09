using KitStack.Audit.AspNetCore.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Health-check registration for KitStack.Audit.
/// </summary>
public static class AuditHealthCheckBuilderExtensions
{
    public static IHealthChecksBuilder AddKitStackAudit(
        this IHealthChecksBuilder builder,
        string name = "audit-sink",
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddCheck<AuditSinkHealthCheck>(name, failureStatus, tags ?? Array.Empty<string>());
    }
}
