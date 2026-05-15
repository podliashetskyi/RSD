using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> b)
    {
        ContentEntityConfiguration.Apply(b, "services");
        b.Property(x => x.Title).HasMaxLength(FieldLimits.Service.Title).IsRequired();
        b.Property(x => x.Summary).HasMaxLength(FieldLimits.Service.Summary);
        b.Property(x => x.Description).HasMaxLength(FieldLimits.Service.Description);
        b.Property(x => x.CoverImagePath).HasMaxLength(FieldLimits.Service.CoverImagePath);
        b.Property(x => x.CoverImageAlt).HasMaxLength(FieldLimits.Service.CoverImageAlt);
        b.Property(x => x.DetailsHref).HasMaxLength(FieldLimits.Service.DetailsHref);
        b.Property(x => x.Intro).HasMaxLength(FieldLimits.Service.Intro);
        b.Property(x => x.BulletPoints).HasColumnType("text[]");

        b.Property(x => x.BodyBlocks)
            .HasColumnType("json")
            .HasConversion(JsonbValueConverter.ConverterFor<ArticleBody>())
            .Metadata.SetValueComparer(JsonbValueConverter.ComparerFor<ArticleBody>());
    }
}
