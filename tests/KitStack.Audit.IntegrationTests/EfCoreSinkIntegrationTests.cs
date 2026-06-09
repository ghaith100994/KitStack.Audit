using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Options;
using KitStack.Audit.EntityFrameworkCore.Extensions;
using KitStack.Audit.EntityFrameworkCore.Interceptors;
using KitStack.Audit.Fakes.Sinks;
using KitStack.Audit.IntegrationTests.Support;
using KitStack.Audit.Sinks.EntityFrameworkCore.Extensions;
using KitStack.Audit.Sinks.EntityFrameworkCore.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KitStack.Audit.IntegrationTests;

/// <summary>
/// End-to-end: capture interceptor -> EfCoreAuditSink -> AuditDbContext, on SQLite.
/// Business and audit stores are separate in-memory SQLite databases.
/// </summary>
public sealed class EfCoreSinkIntegrationTests : IDisposable
{
    private readonly SqliteConnection _businessConnection;
    private readonly SqliteConnection _auditConnection;
    private readonly ServiceProvider _provider;

    public EfCoreSinkIntegrationTests()
    {
        _businessConnection = new SqliteConnection("DataSource=:memory:");
        _businessConnection.Open();
        _auditConnection = new SqliteConnection("DataSource=:memory:");
        _auditConnection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.Configure<AuditOptions>(o => o.EnforceAuditableSetting = false);
        services.AddScoped<IAuditContextAccessor>(_ => new FakeAuditContextAccessor { UserId = Guid.NewGuid() });

        services.AddKitStackAuditCapture();
        services.AddKitStackAuditEntityFrameworkCoreSink(o => o.UseSqlite(_auditConnection));

        services.AddDbContext<BizContext>((sp, o) =>
        {
            o.UseSqlite(_businessConnection);
            o.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<BizContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<AuditDbContext>().Database.EnsureCreated();
    }

    [Fact]
    public async Task Saving_a_new_entity_persists_an_audit_trail()
    {
        var id = Guid.NewGuid();

        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BizContext>();
            db.Orders.Add(new Order { Id = id, Customer = "Acme", Total = 100m });
            await db.SaveChangesAsync();
        }

        await using (var scope = _provider.CreateAsyncScope())
        {
            var audit = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
            var trail = await audit.AuditTrails.SingleAsync();

            Assert.Equal(nameof(Order), trail.Entity);
            Assert.Equal("Create", trail.Operation);
            Assert.Equal("Sales", trail.Module);
            Assert.NotNull(trail.NewValues);
            Assert.Contains("Acme", trail.NewValues!);
        }
    }

    [Fact]
    public async Task A_failed_save_leaves_no_audit_trail()
    {
        var id = Guid.NewGuid();

        // First insert succeeds -> 1 trail.
        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BizContext>();
            db.Orders.Add(new Order { Id = id, Customer = "Acme", Total = 100m });
            await db.SaveChangesAsync();
        }

        // Second insert reuses the same primary key -> the database rejects it.
        await Assert.ThrowsAnyAsync<DbUpdateException>(async () =>
        {
            await using var scope = _provider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<BizContext>();
            db.Orders.Add(new Order { Id = id, Customer = "Duplicate", Total = 1m });
            await db.SaveChangesAsync();
        });

        await using (var scope = _provider.CreateAsyncScope())
        {
            var audit = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
            // Only the first (committed) save was audited; the rolled-back one left nothing.
            Assert.Equal(1, await audit.AuditTrails.CountAsync());
        }
    }

    public void Dispose()
    {
        _provider.Dispose();
        _businessConnection.Dispose();
        _auditConnection.Dispose();
    }
}
