using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class MessengerLinkConfiguration : IEntityTypeConfiguration<MessengerLink>
{
    public void Configure(EntityTypeBuilder<MessengerLink> b)
    {
        ContentEntityConfiguration.Apply(b, "messenger_links");
        b.Property(x => x.Label).HasMaxLength(FieldLimits.MessengerLink.Label).IsRequired();
        b.Property(x => x.LargeIconPath).HasMaxLength(FieldLimits.MessengerLink.LargeIconPath);
        b.Property(x => x.SmallIconPath).HasMaxLength(FieldLimits.MessengerLink.SmallIconPath);
        b.Property(x => x.BgColor).HasMaxLength(FieldLimits.MessengerLink.BgColor);
        b.Property(x => x.Href).HasMaxLength(FieldLimits.MessengerLink.Href);
        b.HasIndex(x => x.DisplayOrder);
    }
}
