using System.Globalization;
using System.Text;
using KitStack.Audit.Abstractions.Enums;
using KitStack.Audit.Abstractions.Models;
using KitStack.Audit.Narration.Contracts;
using KitStack.Audit.Narration.Enums;
using KitStack.Audit.Narration.Models;
using KitStack.Audit.Narration.Services;

namespace KitStack.Audit.Tests.Narration;

public enum ShopOrderStatus
{
    Pending,
    Shipped,
    Cancelled,
}

public sealed class ShopOrder
{
    public Guid Id { get; set; }
    public string Number { get; set; } = "";
    public ShopOrderStatus Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime? DeletedOn { get; set; }
}

/// <summary>The README example narrator.</summary>
public sealed class ShopOrderNarrator(INarrationServices s) : NarratorBase<ShopOrder>(s)
{
    public override ActivityEvent? Narrate(AuditNarrationContext c) =>
        Became(c, nameof(ShopOrder.Status), ShopOrderStatus.Shipped)
            ? Event(c, "truck", Severity.Success, new("Order shipped: " + Val(c, "Number"), "تم شحن الطلب: " + Val(c, "Number")))
            : Modified(c, nameof(ShopOrder.Status))
                ? EnumChange<ShopOrderStatus>(c, nameof(ShopOrder.Status), "list-checks", Severity.Warning, new("Order status", "حالة الطلب"))
                : StandardWithActivation(c, "shopping-cart", new("Order", "طلب"));
}

/// <summary>Exposes the protected helpers to tests.</summary>
public sealed class ProbeNarrator(INarrationServices s) : NarratorBase<ShopOrder>(s)
{
    public override ActivityEvent? Narrate(AuditNarrationContext c) => Standard(c, "box", new("Order", "طلب"));

    public string? V(AuditNarrationContext c, string p) => Val(c, p);
    public T? V<T>(AuditNarrationContext c, string p) => Val<T>(c, p);
    public string NameOf(AuditNarrationContext c) => Name(c);
    public LocalizedText Label<TEnum>(object? value) where TEnum : struct, Enum => EnumLabel<TEnum>(value);
    public bool IsMoved(AuditNarrationContext c) => Moved(c, nameof(ShopOrder.Status), ShopOrderStatus.Pending, ShopOrderStatus.Shipped);
    public bool IsWasIn(AuditNarrationContext c) => WasIn(c, nameof(ShopOrder.Status), ShopOrderStatus.Pending, ShopOrderStatus.Cancelled);
    public IReadOnlyList<string> Fields(AuditNarrationContext c) => UserFields(c);
    public bool IsSoftDeleted(AuditNarrationContext c) => SoftDeleted(c);
    public bool IsRestored(AuditNarrationContext c) => Restored(c);
    public static string? Date(object? v) => When(v);
    public static string? Cut(string? t, int max) => Excerpt(t, max);
    public static string? Joined(params string?[] parts) => Join(parts);
}

/// <summary>Arabic labels the way an ERP adapter for its own label attribute would supply them.</summary>
public sealed class ArabicStatusLabels : IEnumLabelProvider
{
    public LocalizedText GetLabel(Enum value) => value switch
    {
        ShopOrderStatus.Pending => new("Pending", "قيد الانتظار"),
        ShopOrderStatus.Shipped => new("Shipped", "تم الشحن"),
        _ => LocalizedText.Same(value.ToString()),
    };
}

internal static class Trail
{
    public static readonly DateTime At = new(2026, 1, 15, 9, 30, 0, DateTimeKind.Utc);
    public static readonly Guid User = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static AuditNarrationContext Of(
        TrailType type,
        Dictionary<string, object?>? oldValues = null,
        Dictionary<string, object?>? newValues = null,
        params string[] modified)
        => new()
        {
            EntityType = typeof(ShopOrder),
            TrailType = type,
            PrimaryKey = "42",
            Module = "Shop",
            UserId = User,
            Timestamp = At,
            OldValues = oldValues ?? new Dictionary<string, object?>(),
            NewValues = newValues ?? new Dictionary<string, object?>(),
            ModifiedProperties = modified,
        };

    /// <summary>Stable text form of an event, so a wording change shows up as a readable diff.</summary>
    public static string Snapshot(ActivityEvent? e)
    {
        if (e is null)
            return "<null>";

        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"{e.EntityType}#{e.EntityId} {e.Action} module={e.Module}\n");
        sb.Append(CultureInfo.InvariantCulture, $"icon={e.Icon} severity={e.Severity}\n");
        sb.Append(CultureInfo.InvariantCulture, $"en: {e.TitleEn} | {e.SubtitleEn}\n");
        sb.Append(CultureInfo.InvariantCulture, $"ar: {e.TitleAr} | {e.SubtitleAr}\n");
        sb.Append("meta:");
        foreach (var (k, v) in (e.Metadata ?? new Dictionary<string, string>()).OrderBy(p => p.Key, StringComparer.Ordinal))
            sb.Append(CultureInfo.InvariantCulture, $" {k}={v}");
        return sb.ToString();
    }
}
