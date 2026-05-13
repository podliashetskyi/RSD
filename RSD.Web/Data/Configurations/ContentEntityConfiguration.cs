using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

/// <summary>
/// Shared fluent config for every concrete <see cref="ContentEntity"/>: owned Seo,
/// status as string, partial unique index on Slug (filtered to non-deleted rows),
/// and the soft-delete query filter.
/// </summary>
public static class ContentEntityConfiguration
{
    public static void Apply<TEntity>(EntityTypeBuilder<TEntity> b, string tableName)
        where TEntity : ContentEntity
    {
        b.ToTable(tableName);
        b.HasKey(x => x.Id);
        b.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.OwnsOne(x => x.Seo, seo =>
        {
            seo.Property(p => p.MetaTitle).HasMaxLength(200).HasColumnName("SeoMetaTitle");
            seo.Property(p => p.MetaDescription).HasMaxLength(500).HasColumnName("SeoMetaDescription");
            seo.Property(p => p.OgImagePath).HasMaxLength(500).HasColumnName("SeoOgImagePath");
        });

        b.HasIndex(x => x.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = FALSE");

        b.HasIndex(x => x.Status);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
