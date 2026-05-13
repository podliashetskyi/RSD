using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class TechStackItemConfiguration : IEntityTypeConfiguration<TechStackItem>
{
    public void Configure(EntityTypeBuilder<TechStackItem> b)
    {
        ContentEntityConfiguration.Apply(b, "tech_stack_items");
        b.Property(x => x.Label).HasMaxLength(100).IsRequired();
        b.Property(x => x.LogoPath).HasMaxLength(500);
        b.HasIndex(x => x.DisplayOrder);
    }
}
