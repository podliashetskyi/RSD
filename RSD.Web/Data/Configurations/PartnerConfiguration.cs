using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> b)
    {
        ContentEntityConfiguration.Apply(b, "partners");
        b.Property(x => x.Name).HasMaxLength(FieldLimits.Partner.Name).IsRequired();
        b.Property(x => x.Role).HasMaxLength(FieldLimits.Partner.Role);
        b.Property(x => x.PhotoPath).HasMaxLength(FieldLimits.Partner.PhotoPath);
        b.Property(x => x.ContactHref).HasMaxLength(FieldLimits.Partner.ContactHref);
        b.HasIndex(x => x.DisplayOrder);
    }
}
