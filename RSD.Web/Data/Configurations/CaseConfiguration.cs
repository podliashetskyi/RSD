using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> b)
    {
        ContentEntityConfiguration.Apply(b, "cases");
        b.Property(x => x.Name).HasMaxLength(FieldLimits.Case.Name).IsRequired();
        b.Property(x => x.Summary).HasMaxLength(FieldLimits.Case.Summary);
        b.Property(x => x.Industry).HasMaxLength(FieldLimits.Case.Industry);
        b.Property(x => x.Description).HasMaxLength(FieldLimits.Case.Description);
        b.Property(x => x.CoverImagePath).HasMaxLength(FieldLimits.Case.CoverImagePath);
        b.Property(x => x.CoverImageAlt).HasMaxLength(FieldLimits.Case.CoverImageAlt);
        b.Property(x => x.TechTags).HasColumnType("text[]");

        b.Property(x => x.DetailFields)
            .HasColumnType("jsonb")
            .HasConversion(JsonbValueConverter.ConverterFor<CaseDetailFields>())
            .Metadata.SetValueComparer(JsonbValueConverter.ComparerFor<CaseDetailFields>());

        b.HasIndex(x => x.Industry);
    }
}
