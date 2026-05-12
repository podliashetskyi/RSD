using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> b)
    {
        b.ToTable("audit_log_entries");
        b.HasKey(x => x.Id);
        b.Property(x => x.UserId).HasMaxLength(450);
        b.Property(x => x.UserEmail).HasMaxLength(320);
        b.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        b.Property(x => x.Action).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Diff).HasColumnType("jsonb");
        b.HasIndex(x => x.At).IsDescending();
        b.HasIndex(x => x.EntityType);
        b.HasIndex(x => x.UserId);
    }
}
