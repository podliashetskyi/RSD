using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class FilterConfiguration : IEntityTypeConfiguration<Filter>
{
    public void Configure(EntityTypeBuilder<Filter> b)
    {
        ContentEntityConfiguration.Apply(b, "filters");
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(FieldLimits.Filter.Type).IsRequired();
        b.Property(x => x.Label).HasMaxLength(FieldLimits.Filter.Label).IsRequired();
        b.HasIndex(x => x.Type);
        b.HasIndex(x => x.DisplayOrder);
    }
}
