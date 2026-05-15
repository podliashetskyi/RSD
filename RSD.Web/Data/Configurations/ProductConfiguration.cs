using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        ContentEntityConfiguration.Apply(b, "products");
        b.Property(x => x.Name).HasMaxLength(FieldLimits.Product.Name).IsRequired();
        b.Property(x => x.Summary).HasMaxLength(FieldLimits.Product.Summary);
        b.Property(x => x.Subtitle).HasMaxLength(FieldLimits.Product.Subtitle);
        b.Property(x => x.Price).HasMaxLength(FieldLimits.Product.Price);
        b.Property(x => x.Description).HasMaxLength(FieldLimits.Product.Description);
        b.Property(x => x.CoverImagePath).HasMaxLength(FieldLimits.Product.CoverImagePath);
        b.Property(x => x.CoverImageAlt).HasMaxLength(FieldLimits.Product.CoverImageAlt);
        b.Property(x => x.TryForFreeHref).HasMaxLength(FieldLimits.Product.TryForFreeHref);
        b.Property(x => x.LearnMoreHref).HasMaxLength(FieldLimits.Product.LearnMoreHref);
        b.Property(x => x.BulletPoints).HasColumnType("text[]");

        b.Property(x => x.DetailFields)
            .HasColumnType("jsonb")
            .HasConversion(JsonbValueConverter.ConverterFor<ProductDetailFields>())
            .Metadata.SetValueComparer(JsonbValueConverter.ComparerFor<ProductDetailFields>());
    }
}
