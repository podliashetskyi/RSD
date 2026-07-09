using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;

namespace RSD.Web.Services.Storage;

/// <summary>
/// Centralised extractor for every file path stored on a content entity.
/// Only paths under "uploads/" are returned — seed paths under "images/" are static
/// build-time assets and have no UploadedFile row to refcount.
/// </summary>
internal static class EntityPaths
{
    private const string UploadsPrefix = "uploads/";

    public static IEnumerable<string> Of(BlogPost p)
    {
        yield return p.CoverImagePath;
        yield return p.Seo.OgImagePath;
        foreach (var path in FromBody(p.BodyBlocks)) yield return path;
    }

    public static IEnumerable<string> Of(Case c)
    {
        yield return c.CoverImagePath;
        yield return c.Seo.OgImagePath;
        if (c.DetailFields.Testimonial is { } t) yield return t.AvatarPath;
    }

    public static IEnumerable<string> Of(Product p)
    {
        yield return p.CoverImagePath;
        yield return p.Seo.OgImagePath;
    }

    public static IEnumerable<string> Of(Service s)
    {
        yield return s.CoverImagePath;
        yield return s.Seo.OgImagePath;
        foreach (var path in FromBody(s.BodyBlocks)) yield return path;
    }

    public static IEnumerable<string> Of(Testimonial t)
    {
        yield return t.AvatarPath;
        yield return t.Seo.OgImagePath;
    }

    public static IEnumerable<string> Of(TeamMember t)
    {
        yield return t.AvatarPath;
        yield return t.Seo.OgImagePath;
    }

    public static IEnumerable<string> Of(Partner p)
    {
        yield return p.PhotoPath;
        yield return p.Seo.OgImagePath;
    }

    public static IEnumerable<string> Of(Value v)
    {
        yield return v.IconPath;
        yield return v.Seo.OgImagePath;
    }

    public static IEnumerable<string> Of(TechStackItem t)
    {
        yield return t.LogoPath;
        yield return t.Seo.OgImagePath;
    }

    public static IEnumerable<string> Of(MessengerLink m)
    {
        yield return m.LargeIconPath;
        yield return m.SmallIconPath;
        yield return m.Seo.OgImagePath;
    }

    public static IEnumerable<string> Of(SocialLink s)
    {
        yield return s.IconPath;
        yield return s.Seo.OgImagePath;
    }

    public static IEnumerable<string> Of(MissionStat m) => [m.Seo.OgImagePath];
    public static IEnumerable<string> Of(ContactPoint c) => [c.IconPath, c.Seo.OgImagePath];

    public static IEnumerable<string> OfAny(ContentEntity entity) => entity switch
    {
        BlogPost p => Of(p),
        Case c => Of(c),
        Product p => Of(p),
        Service s => Of(s),
        Testimonial t => Of(t),
        TeamMember t => Of(t),
        Partner p => Of(p),
        Value v => Of(v),
        TechStackItem t => Of(t),
        MessengerLink m => Of(m),
        SocialLink s => Of(s),
        MissionStat m => Of(m),
        ContactPoint c => Of(c),
        _ => [],
    };

    public static IEnumerable<string> Tracked(IEnumerable<string> paths) =>
        paths.Where(p => !string.IsNullOrEmpty(p) && p.StartsWith(UploadsPrefix, StringComparison.Ordinal));

    private static IEnumerable<string> FromBody(ArticleBody body)
    {
        foreach (var block in body.Blocks)
        {
            switch (block)
            {
                case ImageBlock i:
                    yield return i.ImagePath;
                    break;
                case GalleryBlock g:
                    foreach (var img in g.Images) yield return img.Src;
                    break;
            }
        }
    }
}
