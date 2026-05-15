using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> b)
    {
        ContentEntityConfiguration.Apply(b, "social_links");
        b.Property(x => x.Label).HasMaxLength(FieldLimits.SocialLink.Label).IsRequired();
        b.Property(x => x.IconPath).HasMaxLength(FieldLimits.SocialLink.IconPath);
        b.Property(x => x.Href).HasMaxLength(FieldLimits.SocialLink.Href);
        b.Property(x => x.Scope).HasConversion<string>().HasMaxLength(FieldLimits.SocialLink.Scope);
        b.HasIndex(x => x.DisplayOrder);
        b.HasIndex(x => x.Scope);
    }
}
