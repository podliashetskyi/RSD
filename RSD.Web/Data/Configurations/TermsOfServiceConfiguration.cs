using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class TermsOfServiceConfiguration : IEntityTypeConfiguration<TermsOfService>
{
    public void Configure(EntityTypeBuilder<TermsOfService> b)
    {
        ContentEntityConfiguration.Apply(b, "terms_of_service");
        b.Property(x => x.Title).HasMaxLength(FieldLimits.TermsOfService.Title).IsRequired();
        b.Property(x => x.LastUpdatedAt).HasColumnType("date");
        b.Property(x => x.BodyHtml).HasColumnType("text");
    }
}
