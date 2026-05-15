using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class TechStackItemConfiguration : IEntityTypeConfiguration<TechStackItem>
{
    public void Configure(EntityTypeBuilder<TechStackItem> b)
    {
        ContentEntityConfiguration.Apply(b, "tech_stack_items");
        b.Property(x => x.Label).HasMaxLength(FieldLimits.TechStackItem.Label).IsRequired();
        b.Property(x => x.LogoPath).HasMaxLength(FieldLimits.TechStackItem.LogoPath);
        b.HasIndex(x => x.DisplayOrder);
    }
}
