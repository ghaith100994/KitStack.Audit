using KitStack.Audit.Abstractions.Models;
using Microsoft.EntityFrameworkCore;

namespace KitStack.Audit.Sinks.Mongo.Persistence;

/// <summary>
/// The audit store's DbContext backed by the MongoDB EF Core provider. It deliberately does NOT
/// implement <c>IAuditableDbContext</c>, so the capture interceptor never audits the audit store.
/// </summary>
public class MongoAuditDbContext : DbContext
{
    public MongoAuditDbContext(DbContextOptions<MongoAuditDbContext> options) : base(options)
    {
        Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
    }

    public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();
    public DbSet<ActivityEvent> Activities => Set<ActivityEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MongoAuditDbContext).Assembly);
    }
}
