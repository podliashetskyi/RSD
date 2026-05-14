using System.Text.Json.Serialization;
using RSD.Web.Data.Entities.ArticleBlocks;

namespace RSD.Web.Data.Entities;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SubsectionBlock), "subsection")]
[JsonDerivedType(typeof(StatsRowBlock),   "stats")]
[JsonDerivedType(typeof(GalleryBlock),    "gallery")]
[JsonDerivedType(typeof(BulletListBlock), "bullets")]
[JsonDerivedType(typeof(QuoteBlock),      "quote")]
[JsonDerivedType(typeof(ImageBlock),      "image")]
[JsonDerivedType(typeof(RichTextBlock),   "richtext")]
public abstract record class ArticleBlock
{
    public required string Id { get; init; }
}
