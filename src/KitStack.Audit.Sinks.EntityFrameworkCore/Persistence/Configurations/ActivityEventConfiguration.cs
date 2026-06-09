using System.Text.Json;
using KitStack.Audit.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KitStack.Audit.Sinks.EntityFrameworkCore.Persistence.Configurations;

/// <summary>
/// Maps <see cref="ActivityEvent"/> to the "ActivityEvents" table. The enum action is stored as a
/// string and the optional metadata dictionary is stored as a JSON string.
/// </summary>
public sealed class ActivityEventConfiguration : IEntityTypeConfiguration<ActivityEvent>
{
    public void Configure(EntityTypeBuilder<ActivityEvent> builder)
    {
        builder.ToTable("ActivityEvents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.EntityType).HasMaxLength(256);
        builder.Property(x => x.EntityId).HasMaxLength(128);
        builder.Property(x => x.Module).HasMaxLength(128);
        builder.Property(x => x.Title).HasMaxLength(512);

        var converter = new ValueConverter<IDictionary<string, string>?, string?>(
            v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null));

        var comparer = new ValueComparer<IDictionary<string, string>?>(
            (l, r) => (l == null && r == null) ||
                      (l != null && r != null && l.Count == r.Count &&
                       l.All(kv => r.ContainsKey(kv.Key) && r[kv.Key] == kv.Value)),
            v => v == null ? 0 : v.Aggregate(0, (acc, kv) => HashCode.Combine(acc, kv.Key, kv.Value)),
            v => v == null ? null : new Dictionary<string, string>(v));

        builder.Property(x => x.Metadata).HasConversion(converter, comparer);

        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => x.UserId);
    }
}
