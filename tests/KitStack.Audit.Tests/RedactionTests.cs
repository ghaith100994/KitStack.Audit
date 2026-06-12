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

public sealed class RedactionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public RedactionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.Configure<AuditOptions>(o =>
        {
            o.EnforceAuditableSetting = false;
            o.ExcludedProperties.Add("Quantity");   // applies to Widget via options
        });
        services.AddKitStackAuditFakes();
        services.AddKitStackAuditCapture();

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
    public async Task Masked_property_is_recorded_with_mask_text()
    {
        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.SecureDocuments.Add(new SecureDocument { Title = "Doc", Password = "p@ss", ApiKey = "key-123" });
            await db.SaveChangesAsync();
        }

        var trail = Assert.Single(Store.Trails);
        Assert.Equal("Doc", trail.NewValues[nameof(SecureDocument.Title)]);
        Assert.Equal("***", trail.NewValues[nameof(SecureDocument.Password)]);
        Assert.Equal("<hidden>", trail.NewValues[nameof(SecureDocument.ApiKey)]);
    }

    [Fact]
    public async Task Masked_null_stays_null()
    {
        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.SecureDocuments.Add(new SecureDocument { Title = "Doc", Password = null });
            await db.SaveChangesAsync();
        }

        var trail = Assert.Single(Store.Trails);
        Assert.Null(trail.NewValues[nameof(SecureDocument.Password)]);
    }

    [Fact]
    public async Task Masked_update_is_listed_as_modified_but_values_are_hidden()
    {
        var id = Guid.NewGuid();
        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.SecureDocuments.Add(new SecureDocument { Id = id, Title = "Doc", Password = "old" });
            await db.SaveChangesAsync();
        }

        Store.Clear();

        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var doc = await db.SecureDocuments.FindAsync(id);
            doc!.Password = "new";
            await db.SaveChangesAsync();
        }

        var trail = Assert.Single(Store.Trails);
        Assert.Equal(TrailType.Update, trail.Type);
        Assert.Contains(nameof(SecureDocument.Password), trail.ModifiedProperties);
        Assert.Equal("***", trail.OldValues[nameof(SecureDocument.Password)]);
        Assert.Equal("***", trail.NewValues[nameof(SecureDocument.Password)]);
    }

    [Fact]
    public async Task Ignored_property_never_appears_in_payloads()
    {
        var id = Guid.NewGuid();
        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.SecureDocuments.Add(new SecureDocument { Id = id, Title = "Doc", SearchCache = "cached" });
            await db.SaveChangesAsync();
        }

        var createTrail = Assert.Single(Store.Trails);
        Assert.False(createTrail.NewValues.ContainsKey(nameof(SecureDocument.SearchCache)));

        Store.Clear();

        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var doc = await db.SecureDocuments.FindAsync(id);
            doc!.SearchCache = "recomputed";
            await db.SaveChangesAsync();
        }

        // Only the ignored property changed, so no trail should be captured at all.
        Assert.Empty(Store.Trails);
    }

    [Fact]
    public async Task Options_excluded_property_is_skipped_for_every_entity()
    {
        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.Widgets.Add(new Widget { Name = "A", Quantity = 7 });
            await db.SaveChangesAsync();
        }

        var trail = Assert.Single(Store.Trails);
        Assert.True(trail.NewValues.ContainsKey(nameof(Widget.Name)));
        Assert.False(trail.NewValues.ContainsKey(nameof(Widget.Quantity)));
    }

    [Fact]
    public async Task Trails_carry_user_context_fields()
    {
        var accessor = (KitStack.Audit.Fakes.Sinks.FakeAuditContextAccessor)
            _provider.GetRequiredService<KitStack.Audit.Abstractions.Contracts.IAuditContextAccessor>();
        accessor.TenantId = "tenant-1";
        accessor.CorrelationId = "corr-42";

        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.Widgets.Add(new Widget { Name = "A", Quantity = 1 });
            await db.SaveChangesAsync();
        }

        var trail = Assert.Single(Store.Trails);
        Assert.Equal("test-user", trail.UserName);
        Assert.Equal("tenant-1", trail.TenantId);
        Assert.Equal("corr-42", trail.CorrelationId);
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
