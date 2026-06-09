using KitStack.Audit.Abstractions.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KitStack.Audit.AspNetCore.HealthChecks;

/// <summary>
/// Liveness check that confirms an <see cref="IAuditSink"/> is registered and resolvable.
/// </summary>
public sealed class AuditSinkHealthCheck : IHealthCheck
{
    private readonly IAuditSink _sink;

    public AuditSinkHealthCheck(IAuditSink sink) => _sink = sink;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(HealthCheckResult.Healthy($"Audit sink: {_sink.GetType().Name}"));
}
