using KitStack.Audit.Abstractions.Models;
using Microsoft.EntityFrameworkCore;

namespace KitStack.Audit.Sinks.EntityFrameworkCore.Persistence;

/// <summary>
/// The audit store's own DbContext. It deliberately does NOT implement
/// <c>IAuditableDbContext</c>, so the capture interceptor never audits the audit store.
/// </summary>
public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
        Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
    }

    public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();
    public DbSet<ActivityEvent> Activities => Set<ActivityEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);
    }
}
