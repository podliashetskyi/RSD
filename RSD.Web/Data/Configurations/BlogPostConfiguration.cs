using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> b)
    {
        ContentEntityConfiguration.Apply(b, "blog_posts");
        b.Property(x => x.Title).HasMaxLength(FieldLimits.BlogPost.Title).IsRequired();
        b.Property(x => x.Summary).HasMaxLength(FieldLimits.BlogPost.Summary);
        b.Property(x => x.Description).HasMaxLength(FieldLimits.BlogPost.Description);
        b.Property(x => x.Category).HasMaxLength(FieldLimits.BlogPost.Category);
        b.Property(x => x.CoverImagePath).HasMaxLength(FieldLimits.BlogPost.CoverImagePath);
        b.Property(x => x.CoverImageAlt).HasMaxLength(FieldLimits.BlogPost.CoverImageAlt);
        b.Property(x => x.Intro).HasMaxLength(FieldLimits.BlogPost.Intro);
        b.Property(x => x.Tags).HasColumnType("text[]");

        b.Property(x => x.BodyBlocks)
            .HasColumnType("json")
            .HasConversion(JsonbValueConverter.ConverterFor<ArticleBody>())
            .Metadata.SetValueComparer(JsonbValueConverter.ComparerFor<ArticleBody>());

        b.HasIndex(x => x.Category);
        b.HasIndex(x => x.AuthorId);
    }
}
