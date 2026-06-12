using KitStack.Audit.Abstractions.Contracts;
using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Models;
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

/// <summary>
/// Covers the committee / committee-member scenario: a change on a child row
/// ([AuditParent]-annotated FK) is also attributed to the owning aggregate, so the
/// parent's profile feed can show it.
/// </summary>
public sealed class ParentRelationTests : IDisposable
{
    /// <summary>Narrator that only sets a title — everything else is filled by the interceptor.</summary>
    private sealed class TitleOnlyDispatcher : IActivityNarrationDispatcher
    {
        public ActivityEvent? Narrate(AuditNarrationContext context)
            => context.EntityType == typeof(CommitteeMember)
                ? new ActivityEvent { TitleEn = $"{context.EntityDisplayEn} added" }
                : null;
    }

    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public ParentRelationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.Configure<AuditOptions>(o => o.EnforceAuditableSetting = false);
        services.AddSingleton<IActivityNarrationDispatcher, TitleOnlyDispatcher>();
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

    private async Task<(Guid CommitteeId, Guid MemberId)> AddCommitteeWithMemberAsync()
    {
        var committee = new Committee { Name = "Finance" };
        var member = new CommitteeMember { CommitteeId = committee.Id, MemberName = "Alice", Role = "Chair" };

        await using var scope = _provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        db.Committees.Add(committee);
        db.CommitteeMembers.Add(member);
        await db.SaveChangesAsync();

        return (committee.Id, member.Id);
    }

    [Fact]
    public async Task Member_trail_is_attributed_to_its_committee()
    {
        var (committeeId, memberId) = await AddCommitteeWithMemberAsync();

        var memberTrail = Assert.Single(Store.Trails, t => t.TableName == nameof(CommitteeMember));
        Assert.Equal(nameof(Committee), memberTrail.RelatedEntityType);
        Assert.Equal(committeeId.ToString(), memberTrail.RelatedEntityId);
        Assert.Equal(memberId.ToString(), memberTrail.EntityId);
        Assert.Equal("Committees", memberTrail.SubModule);
        Assert.Equal("EntityFrameworkCore", memberTrail.Source);

        // The committee's own trail has no parent relation.
        var committeeTrail = Assert.Single(Store.Trails, t => t.TableName == nameof(Committee));
        Assert.Null(committeeTrail.RelatedEntityType);
        Assert.Null(committeeTrail.RelatedEntityId);
    }

    [Fact]
    public async Task Member_delete_keeps_the_parent_attribution()
    {
        var (committeeId, memberId) = await AddCommitteeWithMemberAsync();
        Store.Clear();

        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var member = await db.CommitteeMembers.FindAsync(memberId);
            db.CommitteeMembers.Remove(member!);
            await db.SaveChangesAsync();
        }

        var trail = Assert.Single(Store.Trails);
        Assert.Equal(TrailType.Delete, trail.Type);
        Assert.Equal(nameof(Committee), trail.RelatedEntityType);
        Assert.Equal(committeeId.ToString(), trail.RelatedEntityId);
    }

    [Fact]
    public async Task Narrated_activity_is_auto_filled_and_carries_the_parent_relation()
    {
        var (committeeId, memberId) = await AddCommitteeWithMemberAsync();

        var activity = Assert.Single(Store.Activities);

        // Set by the narrator (using the display name from [AuditDefinition]):
        Assert.Equal("Committee member added", activity.TitleEn);

        // Everything else filled in by the interceptor from the trail:
        Assert.Equal(nameof(CommitteeMember), activity.EntityType);
        Assert.Equal(memberId.ToString(), activity.EntityId);
        Assert.Equal(nameof(Committee), activity.RelatedEntityType);
        Assert.Equal(committeeId.ToString(), activity.RelatedEntityId);
        var relation = Assert.Single(activity.Relations);
        Assert.Equal(nameof(Committee), relation.EntityType);
        Assert.Equal(committeeId.ToString(), relation.EntityId);
        Assert.Equal(TrailType.Create, activity.Action);
        Assert.Equal("Committees", activity.SubModule);
        Assert.Equal("test-user", activity.UserName);
        Assert.NotEqual(default, activity.Timestamp);
    }

    [Fact]
    public async Task Committee_profile_feed_query_returns_both_own_and_member_changes()
    {
        var (committeeId, _) = await AddCommitteeWithMemberAsync();

        // The query an application would run to build the committee profile feed.
        var feed = Store.Trails.Where(t =>
                (t.TableName == nameof(Committee) && t.EntityId == committeeId.ToString()) ||
                (t.RelatedEntityType == nameof(Committee) && t.RelatedEntityId == committeeId.ToString()))
            .ToList();

        Assert.Equal(2, feed.Count);
        Assert.Contains(feed, t => t.TableName == nameof(Committee));
        Assert.Contains(feed, t => t.TableName == nameof(CommitteeMember));
    }

    [Fact]
    public async Task Entity_with_two_parents_is_attributed_to_both()
    {
        var orgId = Guid.NewGuid();
        var deptId = Guid.NewGuid();

        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.People.Add(new Person { OrganizationId = orgId, DepartmentId = deptId, Name = "Alice" });
            await db.SaveChangesAsync();
        }

        var trail = Assert.Single(Store.Trails);

        // Both parents are recorded...
        Assert.Equal(2, trail.Relations.Count);
        Assert.Contains(trail.Relations, r => r.EntityType == "Organization" && r.EntityId == orgId.ToString());
        Assert.Contains(trail.Relations, r => r.EntityType == "Department" && r.EntityId == deptId.ToString());

        // ...and the first [AuditParent] (declaration order) is the primary pair.
        Assert.Equal("Organization", trail.RelatedEntityType);
        Assert.Equal(orgId.ToString(), trail.RelatedEntityId);
    }

    [Fact]
    public async Task Each_parent_feed_finds_the_change_through_relations()
    {
        var orgId = Guid.NewGuid();
        var deptId = Guid.NewGuid();

        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.People.Add(new Person { OrganizationId = orgId, DepartmentId = deptId, Name = "Alice" });
            await db.SaveChangesAsync();
        }

        // The query each profile feed would run.
        static bool InFeed(TrailDto t, string parentType, Guid parentId)
            => t.Relations.Any(r => r.EntityType == parentType && r.EntityId == parentId.ToString());

        Assert.Single(Store.Trails, t => InFeed(t, "Organization", orgId));
        Assert.Single(Store.Trails, t => InFeed(t, "Department", deptId));
        Assert.DoesNotContain(Store.Trails, t => InFeed(t, "Department", orgId));
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
