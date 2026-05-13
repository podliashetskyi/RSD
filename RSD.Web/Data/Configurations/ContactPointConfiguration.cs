using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class ContactPointConfiguration : IEntityTypeConfiguration<ContactPoint>
{
    public void Configure(EntityTypeBuilder<ContactPoint> b)
    {
        ContentEntityConfiguration.Apply(b, "contact_points");
        b.Property(x => x.Label).HasMaxLength(100).IsRequired();
        b.Property(x => x.Lines).HasColumnType("text[]");
        b.HasIndex(x => x.DisplayOrder);
    }
}
