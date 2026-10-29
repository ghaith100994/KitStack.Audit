# KitStack.Audit.Narration

Bilingual (English / Arabic) narration toolkit for [KitStack.Audit](https://github.com/ghaith100994/KitStack.Audit).

`AuditNarratorBase<T>` only decides which entity a narrator handles. `NarratorBase<T>` adds the helpers every activity feed ends up rewriting:

- value readers (`Val`, `Val<T>`, `ValAny`, `OldVal`, `NewVal<T>`, `BoolVal`, `IntVal`, `LongVal`, `DateTimeVal`, `GuidVal`)
- enum labels and transitions (`EnumLabel<TEnum>`, `NewEnum`, `OldEnum`, `Became`, `Moved`, `WasIn`)
- change detection (`Modified`, `Changed`, `UserFields`, `SoftDeleted`, `Restored`, `Removed`)
- formatting and metadata (`Name`, `When`, `Excerpt`, `Join`, `Meta`, `ChangedFrom`)
- an event builder (`Event`, `EnumChange`) and the standard templates (`Standard`, `StandardWithActivation`), which produce created / updated / removed / restored / activated / deactivated events

## Install

```bash
dotnet add package KitStack.Audit.Narration
```

It depends only on `KitStack.Audit.Abstractions` and `Microsoft.Extensions.DependencyInjection.Abstractions`.

## Usage

```csharp
services.AddKitStackAudit<IAuditable>(builder.Configuration.GetSection("Audit"))
        .AddKitStackAuditNarration()                       // the dispatcher (KitStack.Audit.AspNetCore)
        .AddKitStackAuditNarrationToolkit(o => o.SystemFields.Add("RowVersion"))
        .AddAuditNarratorsFromAssembly(typeof(OrderNarrator).Assembly);

sealed class OrderNarrator(INarrationServices s) : NarratorBase<Order>(s)
{
    public override ActivityEvent? Narrate(AuditNarrationContext c) =>
        Became(c, nameof(Order.Status), OrderStatus.Shipped)
            ? Event(c, "truck", Severity.Success, new("Order shipped: " + Val(c, "Number"), "تم شحن الطلب: " + Val(c, "Number")))
            : Standard(c, "shopping-cart", new("Order", "طلب"));
}
```

`Standard` gives:

| Trail | Icon | Severity | English | Arabic |
|---|---|---|---|---|
| Create | your icon (or `createIcon`) | success | `Order created: SO-1` | `تم إنشاء طلب: SO-1` |
| Update (user fields) | your icon | info | `Order updated: SO-1` / `2 field(s) changed` | `تم تحديث طلب: SO-1` / `تم تعديل 2 حقل` |
| Update (system fields only) | none (returns `null`) | | | |
| Delete or soft delete | `trash` | critical | `Order removed: SO-1` | `تم حذف طلب: SO-1` |
| Restore | `undo-2` | success | `Order restored: SO-1` | `تمت استعادة طلب: SO-1` |

`StandardWithActivation` also maps a change of `IsActive` to `activated` (`toggle-right`, success) or `deactivated` (`toggle-left`, warning).

`AddAuditNarratorsFromAssembly(assembly, baseNarratorType)` registers every concrete `IAuditNarrator` in the assembly as a singleton, without adding the same narrator twice. Pass a base type to narrow the scan. It can be closed or an open generic such as `typeof(MyModuleNarrator<>)`.

## Configuration

| Setting | Default | Purpose |
|---|---|---|
| `NarrationOptions.SystemFields` | `Created*`, `LastModified*`, `Deleted*`, `ActivationChanged*` (`By`, `ByName`, `On`) | Ignored by `UserFields`; an update touching only these is not narrated |
| `NarrationOptions.NameProperties` | `Number`, `NameAr`, `NameEn`, `Title`, `Name`, `Code` | Tried in order by `Name(c)`; the primary key is the fallback |
| `NarrationOptions.SoftDeleteProperty` | `DeletedOn` | Set means soft-deleted, cleared means restored |
| `NarrationOptions.ActivationProperty` | `IsActive` | Read by `StandardWithActivation` |
| `NarrationOptions.Icons` | `trash`, `undo-2`, `toggle-right`, `toggle-left` | Icons of the standard templates |

A narrator base can override `NameProperties` (or `Name`) to use its own list.

### Enum labels

`IEnumLabelProvider` turns an enum value into `LocalizedText`. The default uses `[Description]`, or the value's name, in both languages. To supply real Arabic labels, plug in your own provider:

```csharp
public sealed class DescriptionArEnLabelProvider : IEnumLabelProvider
{
    public LocalizedText GetLabel(Enum value)
    {
        var a = value.GetType().GetField(value.ToString())?.GetCustomAttribute<DescriptionArEnAttribute>();
        return a is null ? LocalizedText.Same(value.ToString()) : new(a.EnglishName, a.ArabicName);
    }
}

services.AddNarrationEnumLabelProvider<DescriptionArEnLabelProvider>();
```

### Phrases

The sentence templates live in `INarrationPhrases`, and `DefaultNarrationPhrases` holds the wording shown above. To reword a single phrase, derive from it, override that member, and register the result with `services.AddNarrationPhrases<MyPhrases>()`.

> Narrator output is persisted. If you change a phrase, new history reads differently from old history. The snapshot tests in `tests/KitStack.Audit.Tests/Narration` lock the default wording.

### Severity

`Severity` (`Info`, `Success`, `Warning`, `Critical`) is stored on `ActivityEvent.Severity` as `info` / `success` / `warning` / `critical`, which matches existing rows. `SeverityExtensions.ParseSeverity` reads a stored value back.

## Migrating a hand-written module base

1. Derive the module base from `NarratorBase<TEntity>` and delete the copied helpers. Keep only module-specific ones, such as a `ModuleMeta(c)` builder.
2. Add a constructor: `protected MyModuleNarrator(INarrationServices s) : base(s) { }`. Concrete narrators take `INarrationServices` too.
3. If the module named entities from a different property list, override `NameProperties` with the old list so titles stay the same.
4. Replace `(labelEn, labelAr)` string pairs with `new LocalizedText(en, ar)`, and `"success"`-style strings with `Severity.Success`.
5. Enum helpers now return `LocalizedText(En, Ar)` instead of the `(Ar, En)` tuple. Use `.En` / `.Ar` rather than deconstructing.
6. `Undeleted(c)` is now `Restored(c)`.
7. Replace the module's reflection scan with `AddAuditNarratorsFromAssembly(typeof(X).Assembly, typeof(MyModuleNarrator<>))`.
