using KitStack.Audit.EntityFrameworkCore.Contracts;
using Microsoft.EntityFrameworkCore;

namespace KitStack.Audit.Tests.Support;

public class TestDbContext : DbContext, IAuditableDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<Widget> Widgets => Set<Widget>();

    // Settable so a single test can flip auditing off.
    public bool EnableAuditing { get; set; } = true;
    public string? AuditModule => "Test";
}
