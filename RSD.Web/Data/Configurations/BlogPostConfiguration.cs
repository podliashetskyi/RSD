using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RSD.Web.Data.Entities;

namespace RSD.Web.Data.Configurations;

public sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> b)
    {
        ContentEntityConfiguration.Apply(b, "blog_posts");
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.Category).HasMaxLength(100);
        b.Property(x => x.CoverImagePath).HasMaxLength(500);
        b.Property(x => x.CoverImageAlt).HasMaxLength(200);
        b.Property(x => x.Intro).HasMaxLength(4000);
        b.Property(x => x.Tags).HasColumnType("text[]");

        b.Property(x => x.BodyBlocks)
            .HasColumnType("json")
            .HasConversion(JsonbValueConverter.ConverterFor<ArticleBody>())
            .Metadata.SetValueComparer(JsonbValueConverter.ComparerFor<ArticleBody>());

        b.HasIndex(x => x.Category);
        b.HasIndex(x => x.AuthorId);
    }
}
