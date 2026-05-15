using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class PrivacyPolicyConfiguration : IEntityTypeConfiguration<PrivacyPolicy>
{
    public void Configure(EntityTypeBuilder<PrivacyPolicy> b)
    {
        ContentEntityConfiguration.Apply(b, "privacy_policies");
        b.Property(x => x.Title).HasMaxLength(FieldLimits.PrivacyPolicy.Title).IsRequired();
        b.Property(x => x.LastUpdatedAt).HasColumnType("date");
        b.Property(x => x.BodyHtml).HasColumnType("text");
    }
}
