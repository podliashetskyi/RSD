using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class ValueConfiguration : IEntityTypeConfiguration<Value>
{
    public void Configure(EntityTypeBuilder<Value> b)
    {
        ContentEntityConfiguration.Apply(b, "values");
        b.Property(x => x.Title).HasMaxLength(FieldLimits.Value.Title).IsRequired();
        b.Property(x => x.Description).HasMaxLength(FieldLimits.Value.Description);
        b.Property(x => x.IconPath).HasMaxLength(FieldLimits.Value.IconPath);
        b.HasIndex(x => x.DisplayOrder);
    }
}
