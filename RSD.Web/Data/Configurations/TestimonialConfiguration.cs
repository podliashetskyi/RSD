using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> b)
    {
        ContentEntityConfiguration.Apply(b, "testimonials");
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Quote).HasMaxLength(2000).IsRequired();
        b.Property(x => x.AvatarPath).HasMaxLength(500);
        b.Property(x => x.AuthorName).HasMaxLength(200).IsRequired();
        b.Property(x => x.AuthorRole).HasMaxLength(200);
        b.HasIndex(x => x.DisplayOrder);
    }
}
