using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        ContentEntityConfiguration.Apply(b, "products");
        b.Property(x => x.Name).HasMaxLength(300).IsRequired();
        b.Property(x => x.Subtitle).HasMaxLength(300);
        b.Property(x => x.Price).HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.CoverImagePath).HasMaxLength(500);
        b.Property(x => x.TryForFreeHref).HasMaxLength(500);
        b.Property(x => x.LearnMoreHref).HasMaxLength(500);
        b.Property(x => x.BulletPoints).HasColumnType("text[]");

        b.Property(x => x.DetailFields)
            .HasColumnType("jsonb")
            .HasConversion(JsonbValueConverter.ConverterFor<ProductDetailFields>())
            .Metadata.SetValueComparer(JsonbValueConverter.ComparerFor<ProductDetailFields>());
    }
}
