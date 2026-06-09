using System.Text.Json;
using KitStack.Audit.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MongoDB.EntityFrameworkCore.Extensions;

namespace KitStack.Audit.Sinks.Mongo.Persistence.Configurations;

/// <summary>
/// Maps <see cref="ActivityEvent"/> to the "ActivityEvents" collection. The key maps to "_id",
/// the enum action is stored as a string, and metadata is stored as a JSON string.
/// </summary>
public sealed class ActivityEventConfiguration : IEntityTypeConfiguration<ActivityEvent>
{
    public void Configure(EntityTypeBuilder<ActivityEvent> builder)
    {
        builder.ToCollection("ActivityEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasElementName("_id");
        builder.Property(x => x.Action).HasConversion<string>();

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
    }
}
