using KitStack.Audit.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;

namespace KitStack.Audit.Sinks.Mongo.Persistence.Configurations;

/// <summary>
/// Maps <see cref="AuditTrail"/> to the "AuditTrails" collection. The key maps to Mongo's "_id".
/// Value payloads are stored as JSON strings (see <c>TrailDto.ToAuditTrail()</c>), because EF Core
/// cannot map the arbitrary value dictionaries directly.
/// </summary>
public sealed class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.ToCollection("AuditTrails");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasElementName("_id");
    }
}
