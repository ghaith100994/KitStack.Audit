using KitStack.Audit.Abstractions.Models;
using Microsoft.EntityFrameworkCore;

namespace KitStack.Audit.Sinks.Mongo.Persistence;

/// <summary>
/// The audit store's DbContext backed by the MongoDB EF Core provider.
/// It deliberately does NOT implement <c>IAuditableDbContext</c>, so the capture interceptor
/// never audits the audit store.
/// </summary>
public class MongoAuditDbContext : DbContext
{
    public MongoAuditDbContext(DbContextOptions<MongoAuditDbContext> options) : base(options)
    {
        // Standalone MongoDB does not support multi-document transactions; trails are appended
        // as independent documents, so skip EF's implicit transaction.
        Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
    }

    public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MongoAuditDbContext).Assembly);
    }
}
