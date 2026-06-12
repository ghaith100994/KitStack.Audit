using KitStack.Audit.EntityFrameworkCore.Contracts;
using Microsoft.EntityFrameworkCore;

namespace KitStack.Audit.Tests.Support;

public class TestDbContext : DbContext, IAuditableDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<Widget> Widgets => Set<Widget>();
    public DbSet<SecureDocument> SecureDocuments => Set<SecureDocument>();
    public DbSet<Committee> Committees => Set<Committee>();
    public DbSet<CommitteeMember> CommitteeMembers => Set<CommitteeMember>();
    public DbSet<Person> People => Set<Person>();

    // Settable so a single test can flip auditing off.
    public bool EnableAuditing { get; set; } = true;
    public string? AuditModule => "Test";
}
