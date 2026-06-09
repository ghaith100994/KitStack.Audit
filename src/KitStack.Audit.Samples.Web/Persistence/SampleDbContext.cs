using KitStack.Audit.EntityFrameworkCore.Contracts;
using KitStack.Audit.Samples.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace KitStack.Audit.Samples.Web.Persistence;

public class SampleDbContext : DbContext, IAuditableDbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    // IAuditableDbContext
    public bool EnableAuditing => true;
    public string? AuditModule => "Catalog";
}
