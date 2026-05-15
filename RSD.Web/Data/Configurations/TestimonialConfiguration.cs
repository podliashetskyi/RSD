using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> b)
    {
        ContentEntityConfiguration.Apply(b, "testimonials");
        b.Property(x => x.Title).HasMaxLength(FieldLimits.Testimonial.Title).IsRequired();
        b.Property(x => x.Quote).HasMaxLength(FieldLimits.Testimonial.Quote).IsRequired();
        b.Property(x => x.AvatarPath).HasMaxLength(FieldLimits.Testimonial.AvatarPath);
        b.Property(x => x.AuthorName).HasMaxLength(FieldLimits.Testimonial.AuthorName).IsRequired();
        b.Property(x => x.AuthorRole).HasMaxLength(FieldLimits.Testimonial.AuthorRole);
        b.HasIndex(x => x.DisplayOrder);
    }
}
