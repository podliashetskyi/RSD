using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class FaqItemConfiguration : IEntityTypeConfiguration<FaqItem>
{
    public void Configure(EntityTypeBuilder<FaqItem> b)
    {
        ContentEntityConfiguration.Apply(b, "faq_items");
        b.Property(x => x.Question).HasMaxLength(FieldLimits.FaqItem.Question).IsRequired();
        b.Property(x => x.AnswerHtml).HasColumnType("text");
        b.Property(x => x.OwnerSlug).HasMaxLength(FieldLimits.FaqItem.OwnerSlug);
        b.Property(x => x.Category).HasMaxLength(FieldLimits.FaqItem.Category);
        b.HasIndex(x => x.DisplayOrder);
        b.HasIndex(x => x.OwnerSlug);
    }
}
