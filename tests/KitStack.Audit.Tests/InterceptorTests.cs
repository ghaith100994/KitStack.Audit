using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Options;
using KitStack.Audit.EntityFrameworkCore.Extensions;
using KitStack.Audit.EntityFrameworkCore.Interceptors;
using KitStack.Audit.Fakes.Contracts;
using KitStack.Audit.Fakes.Extensions;
using KitStack.Audit.Tests.Support;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KitStack.Audit.Tests;

public sealed class InterceptorTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public InterceptorTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.Configure<AuditOptions>(o => o.EnforceAuditableSetting = false);
        services.AddKitStackAuditFakes();   // in-memory sink + fake accessor + allow-all registry
        services.AddKitStackAuditCapture();  // interceptor + default (audit-all) filter

        services.AddDbContext<TestDbContext>((sp, o) =>
        {
            o.UseSqlite(_connection);
            o.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<TestDbContext>().Database.EnsureCreated();
    }

    private IFakeAuditStore Store => _provider.GetRequiredService<IFakeAuditStore>();

    [Fact]
    public async Task Create_produces_a_create_trail()
    {
        var id = Guid.NewGuid();
        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.Widgets.Add(new Widget { Id = id, Name = "A", Quantity = 1 });
            await db.SaveChangesAsync();
        }

        var trail = Assert.Single(Store.Trails);
        Assert.Equal(TrailType.Create, trail.Type);
        Assert.Equal(nameof(Widget), trail.TableName);
        Assert.True(trail.NewValues.ContainsKey(nameof(Widget.Name)));
        Assert.True(trail.KeyValues.ContainsKey(nameof(Widget.Id)));
    }

    [Fact]
    public async Task Update_records_only_changed_properties()
    {
        var id = Guid.NewGuid();
        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.Widgets.Add(new Widget { Id = id, Name = "A", Quantity = 1 });
            await db.SaveChangesAsync();
        }

        Store.Clear();

        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var widget = await db.Widgets.FindAsync(id);
            widget!.Name = "B";
            await db.SaveChangesAsync();
        }

        var trail = Assert.Single(Store.Trails);
        Assert.Equal(TrailType.Update, trail.Type);
        Assert.Contains(nameof(Widget.Name), trail.ModifiedProperties);
        Assert.DoesNotContain(nameof(Widget.Quantity), trail.ModifiedProperties);
        Assert.Equal("A", trail.OldValues[nameof(Widget.Name)]);
        Assert.Equal("B", trail.NewValues[nameof(Widget.Name)]);
    }

    [Fact]
    public async Task Delete_produces_a_delete_trail()
    {
        var id = Guid.NewGuid();
        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.Widgets.Add(new Widget { Id = id, Name = "A", Quantity = 1 });
            await db.SaveChangesAsync();
        }

        Store.Clear();

        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var widget = await db.Widgets.FindAsync(id);
            db.Widgets.Remove(widget!);
            await db.SaveChangesAsync();
        }

        var trail = Assert.Single(Store.Trails);
        Assert.Equal(TrailType.Delete, trail.Type);
        Assert.True(trail.OldValues.ContainsKey(nameof(Widget.Name)));
    }

    [Fact]
    public async Task Disabled_context_produces_no_trails()
    {
        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.EnableAuditing = false;
            db.Widgets.Add(new Widget { Id = Guid.NewGuid(), Name = "X", Quantity = 5 });
            await db.SaveChangesAsync();
        }

        Assert.Empty(Store.Trails);
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
