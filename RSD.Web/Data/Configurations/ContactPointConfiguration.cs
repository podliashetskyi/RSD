using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class ContactPointConfiguration : IEntityTypeConfiguration<ContactPoint>
{
    public void Configure(EntityTypeBuilder<ContactPoint> b)
    {
        ContentEntityConfiguration.Apply(b, "contact_points");
        b.Property(x => x.Label).HasMaxLength(FieldLimits.ContactPoint.Label).IsRequired();
        b.Property(x => x.Lines).HasColumnType("text[]");
        b.Property(x => x.Href).HasMaxLength(FieldLimits.ContactPoint.Href);
        b.Property(x => x.IconPath).HasMaxLength(FieldLimits.ContactPoint.IconPath);
        b.HasIndex(x => x.DisplayOrder);
    }
}
