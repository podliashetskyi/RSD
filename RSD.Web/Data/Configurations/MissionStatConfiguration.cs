using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class MissionStatConfiguration : IEntityTypeConfiguration<MissionStat>
{
    public void Configure(EntityTypeBuilder<MissionStat> b)
    {
        ContentEntityConfiguration.Apply(b, "mission_stats");
        b.Property(x => x.Label).HasMaxLength(FieldLimits.MissionStat.Label).IsRequired();
        b.Property(x => x.Number).HasMaxLength(FieldLimits.MissionStat.Number);
        b.Property(x => x.Symbol).HasMaxLength(FieldLimits.MissionStat.Symbol);
        b.HasIndex(x => x.DisplayOrder);
    }
}
