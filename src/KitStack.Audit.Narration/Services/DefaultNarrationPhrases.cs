using System.Globalization;
using KitStack.Audit.Narration.Contracts;
using KitStack.Audit.Narration.Models;

namespace KitStack.Audit.Narration.Services;

/// <summary>
/// The English/Arabic templates the ERP narrators used. Derive and override single members
/// to reword one phrase without re-implementing the rest.
/// </summary>
public class DefaultNarrationPhrases : INarrationPhrases
{
    public virtual string EmptyValue => "—";

    public virtual LocalizedText Created(LocalizedText label, string name)
        => new($"{label.En} created: {name}", $"تم إنشاء {label.Ar}: {name}");

    public virtual LocalizedText Updated(LocalizedText label, string name)
        => new($"{label.En} updated: {name}", $"تم تحديث {label.Ar}: {name}");

    public virtual LocalizedText Removed(LocalizedText label, string name)
        => new($"{label.En} removed: {name}", $"تم حذف {label.Ar}: {name}");

    public virtual LocalizedText Restored(LocalizedText label, string name)
        => new($"{label.En} restored: {name}", $"تمت استعادة {label.Ar}: {name}");

    public virtual LocalizedText Activated(LocalizedText label, string name)
        => new($"{label.En} activated: {name}", $"تم تفعيل {label.Ar}: {name}");

    public virtual LocalizedText Deactivated(LocalizedText label, string name)
        => new($"{label.En} deactivated: {name}", $"تم تعطيل {label.Ar}: {name}");

    public virtual LocalizedText Changed(LocalizedText label, string name)
        => new($"{label.En} changed: {name}", $"تغيّر {label.Ar}: {name}");

    public virtual LocalizedText FieldsChanged(int count)
        => new(
            string.Create(CultureInfo.InvariantCulture, $"{count} field(s) changed"),
            string.Create(CultureInfo.InvariantCulture, $"تم تعديل {count} حقل"));

    public virtual LocalizedText StatusActive => new("Status → Active", "الحالة ← نشط");

    public virtual LocalizedText StatusInactive => new("Status → Inactive", "الحالة ← غير نشط");

    public virtual LocalizedText Transition(LocalizedText from, LocalizedText to)
        => new($"{from.En} → {to.En}", $"{from.Ar} ← {to.Ar}");
}
