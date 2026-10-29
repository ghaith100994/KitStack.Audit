using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Narration.Services;
using Xunit;

namespace KitStack.Audit.Tests.Narration;

/// <summary>
/// Narrator output is persisted, so these lock the exact wording, icons and severities the ERP
/// narrators produced. A failure here means new history will read differently from old history.
/// </summary>
public class NarrationSnapshotTests
{
    private readonly ShopOrderNarrator _narrator = new(NarrationServices.CreateDefault());

    [Fact]
    public void Create()
        => Assert.Equal(
            """
            ShopOrder#42 Create module=Shop
            icon=shopping-cart severity=success
            en: Order created: SO-1 | 
            ar: تم إنشاء طلب: SO-1 | 
            meta:
            """,
            Trail.Snapshot(_narrator.Narrate(Trail.Of(TrailType.Create, newValues: new() { ["Number"] = "SO-1" }))));

    [Fact]
    public void Update_of_user_fields()
        => Assert.Equal(
            """
            ShopOrder#42 Update module=Shop
            icon=shopping-cart severity=info
            en: Order updated: SO-1 | 2 field(s) changed
            ar: تم تحديث طلب: SO-1 | تم تعديل 2 حقل
            meta: fields=Number, Note
            """,
            Trail.Snapshot(_narrator.Narrate(Trail.Of(TrailType.Update,
                newValues: new() { ["Number"] = "SO-1" },
                modified: ["Number", "Note", "LastModifiedOn", "LastModifiedBy"]))));

    [Fact]
    public void Update_of_system_fields_only_is_not_narrated()
        => Assert.Null(_narrator.Narrate(Trail.Of(TrailType.Update,
            newValues: new() { ["Number"] = "SO-1" },
            modified: ["LastModifiedOn", "LastModifiedBy"])));

    [Fact]
    public void Hard_delete()
        => Assert.Equal(
            """
            ShopOrder#42 Delete module=Shop
            icon=trash severity=critical
            en: Order removed: SO-1 | 
            ar: تم حذف طلب: SO-1 | 
            meta:
            """,
            Trail.Snapshot(_narrator.Narrate(Trail.Of(TrailType.Delete, oldValues: new() { ["Number"] = "SO-1" }))));

    [Fact]
    public void Soft_delete()
        => Assert.Equal(
            """
            ShopOrder#42 Update module=Shop
            icon=trash severity=critical
            en: Order removed: SO-1 | 
            ar: تم حذف طلب: SO-1 | 
            meta:
            """,
            Trail.Snapshot(_narrator.Narrate(Trail.Of(TrailType.Update,
                newValues: new() { ["Number"] = "SO-1", ["DeletedOn"] = Trail.At },
                modified: ["DeletedOn", "DeletedBy"]))));

    [Fact]
    public void Restore()
        => Assert.Equal(
            """
            ShopOrder#42 Update module=Shop
            icon=undo-2 severity=success
            en: Order restored: SO-1 | 
            ar: تمت استعادة طلب: SO-1 | 
            meta:
            """,
            Trail.Snapshot(_narrator.Narrate(Trail.Of(TrailType.Update,
                oldValues: new() { ["DeletedOn"] = Trail.At },
                newValues: new() { ["Number"] = "SO-1", ["DeletedOn"] = null },
                modified: ["DeletedOn"]))));

    [Fact]
    public void Activate()
        => Assert.Equal(
            """
            ShopOrder#42 Update module=Shop
            icon=toggle-right severity=success
            en: Order activated: SO-1 | Status → Active
            ar: تم تفعيل طلب: SO-1 | الحالة ← نشط
            meta:
            """,
            Trail.Snapshot(_narrator.Narrate(Trail.Of(TrailType.Update,
                newValues: new() { ["Number"] = "SO-1", ["IsActive"] = true },
                modified: ["IsActive"]))));

    [Fact]
    public void Deactivate()
        => Assert.Equal(
            """
            ShopOrder#42 Update module=Shop
            icon=toggle-left severity=warning
            en: Order deactivated: SO-1 | Status → Inactive
            ar: تم تعطيل طلب: SO-1 | الحالة ← غير نشط
            meta:
            """,
            Trail.Snapshot(_narrator.Narrate(Trail.Of(TrailType.Update,
                newValues: new() { ["Number"] = "SO-1", ["IsActive"] = false },
                modified: ["IsActive"]))));

    [Fact]
    public void Custom_event_when_status_became_shipped()
        => Assert.Equal(
            """
            ShopOrder#42 Update module=Shop
            icon=truck severity=success
            en: Order shipped: SO-1 | 
            ar: تم شحن الطلب: SO-1 | 
            meta:
            """,
            Trail.Snapshot(_narrator.Narrate(Trail.Of(TrailType.Update,
                oldValues: new() { ["Status"] = "Pending" },
                newValues: new() { ["Number"] = "SO-1", ["Status"] = "Shipped" },
                modified: ["Status"]))));

    [Fact]
    public void Enum_change_with_default_labels()
        => Assert.Equal(
            """
            ShopOrder#42 Update module=Shop
            icon=list-checks severity=warning
            en: Order status changed: SO-1 | Shipped → Cancelled
            ar: تغيّر حالة الطلب: SO-1 | Shipped ← Cancelled
            meta: from=Shipped property=Status to=Cancelled
            """,
            Trail.Snapshot(_narrator.Narrate(Trail.Of(TrailType.Update,
                oldValues: new() { ["Status"] = "Shipped" },
                newValues: new() { ["Number"] = "SO-1", ["Status"] = "Cancelled" },
                modified: ["Status"]))));

    [Fact]
    public void Enum_change_with_bilingual_label_provider()
    {
        var narrator = new ShopOrderNarrator(NarrationServices.CreateDefault(enumLabels: new ArabicStatusLabels()));
        Assert.Equal(
            """
            ShopOrder#42 Update module=Shop
            icon=list-checks severity=warning
            en: Order status changed: SO-1 | Shipped → Pending
            ar: تغيّر حالة الطلب: SO-1 | تم الشحن ← قيد الانتظار
            meta: from=Shipped property=Status to=Pending
            """,
            Trail.Snapshot(narrator.Narrate(Trail.Of(TrailType.Update,
                oldValues: new() { ["Status"] = ShopOrderStatus.Shipped },
                newValues: new() { ["Number"] = "SO-1", ["Status"] = ShopOrderStatus.Pending },
                modified: ["Status"]))));
    }

    [Fact]
    public void Name_falls_back_to_primary_key()
        => Assert.Equal(
            """
            ShopOrder#42 Create module=Shop
            icon=shopping-cart severity=success
            en: Order created: 42 | 
            ar: تم إنشاء طلب: 42 | 
            meta:
            """,
            Trail.Snapshot(_narrator.Narrate(Trail.Of(TrailType.Create))));

    [Fact]
    public void Event_copies_trail_identity()
    {
        var e = _narrator.Narrate(Trail.Of(TrailType.Create, newValues: new() { ["Number"] = "SO-1" }))!;
        Assert.Equal(Trail.User, e.UserId);
        Assert.Equal(Trail.At, e.Timestamp);
    }
}
