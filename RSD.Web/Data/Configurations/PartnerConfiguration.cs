using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> b)
    {
        ContentEntityConfiguration.Apply(b, "partners");
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Role).HasMaxLength(200);
        b.Property(x => x.PhotoPath).HasMaxLength(500);
        b.Property(x => x.ContactHref).HasMaxLength(500);
        b.HasIndex(x => x.DisplayOrder);
    }
}
