using KitStack.Audit.EntityFrameworkCore.Contracts;
using Microsoft.EntityFrameworkCore;

namespace KitStack.Audit.IntegrationTests.Support;

public class BizContext : DbContext, IAuditableDbContext
{
    public BizContext(DbContextOptions<BizContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    public bool EnableAuditing => true;
    public string? AuditModule => "Sales";
}
