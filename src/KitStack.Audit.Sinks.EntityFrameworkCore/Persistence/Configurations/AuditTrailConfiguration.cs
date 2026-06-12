using KitStack.Audit.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KitStack.Audit.Sinks.EntityFrameworkCore.Persistence.Configurations;

/// <summary>
/// Maps <see cref="AuditTrail"/> to the "AuditTrails" table. The JSON payload columns are
/// left unbounded so the provider picks its large-text type (nvarchar(max), text, jsonb-as-text, ...).
/// </summary>
public sealed class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.ToTable("AuditTrails");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Operation).HasMaxLength(32);
        builder.Property(x => x.EntityId).HasMaxLength(128);
        builder.Property(x => x.SubModule).HasMaxLength(128);
        builder.Property(x => x.Source).HasMaxLength(64);
        builder.Property(x => x.RelatedEntityType).HasMaxLength(256);
        builder.Property(x => x.RelatedEntityId).HasMaxLength(128);
        builder.Property(x => x.UserName).HasMaxLength(256);
        builder.Property(x => x.TenantId).HasMaxLength(128);
        builder.Property(x => x.CorrelationId).HasMaxLength(128);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.Entity).HasMaxLength(256);
        builder.Property(x => x.Module).HasMaxLength(128);

        // JSON payloads (set by TrailDto.ToAuditTrail()).
        builder.Property(x => x.PreviousValues);
        builder.Property(x => x.NewValues);
        builder.Property(x => x.ModifiedProperties);
        builder.Property(x => x.PrimaryKey);

        // Common query paths.
        builder.HasIndex(x => x.DateTime);
        builder.HasIndex(x => x.Entity);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.Entity, x.EntityId });
        builder.HasIndex(x => new { x.RelatedEntityType, x.RelatedEntityId });
    }
}
