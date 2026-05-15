using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> b)
    {
        ContentEntityConfiguration.Apply(b, "cases");
        b.Property(x => x.Name).HasMaxLength(300).IsRequired();
        b.Property(x => x.Industry).HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.CoverImagePath).HasMaxLength(500);
        b.Property(x => x.CoverImageAlt).HasMaxLength(200);
        b.Property(x => x.TechTags).HasColumnType("text[]");

        b.Property(x => x.DetailFields)
            .HasColumnType("jsonb")
            .HasConversion(JsonbValueConverter.ConverterFor<CaseDetailFields>())
            .Metadata.SetValueComparer(JsonbValueConverter.ComparerFor<CaseDetailFields>());

        b.HasIndex(x => x.Industry);
    }
}
