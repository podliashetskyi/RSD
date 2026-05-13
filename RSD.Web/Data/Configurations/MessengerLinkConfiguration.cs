using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class MessengerLinkConfiguration : IEntityTypeConfiguration<MessengerLink>
{
    public void Configure(EntityTypeBuilder<MessengerLink> b)
    {
        ContentEntityConfiguration.Apply(b, "messenger_links");
        b.Property(x => x.Label).HasMaxLength(100).IsRequired();
        b.Property(x => x.LargeIconPath).HasMaxLength(500);
        b.Property(x => x.SmallIconPath).HasMaxLength(500);
        b.Property(x => x.BgColor).HasMaxLength(20);
        b.Property(x => x.Href).HasMaxLength(500);
        b.HasIndex(x => x.DisplayOrder);
    }
}
