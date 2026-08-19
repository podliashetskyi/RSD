using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Seo;

/// <summary>Fallback chains for detail-page head values: admin SEO fields win, content fields fill gaps.</summary>
public static class SeoFallbacks
{
    public static string Title(SeoMetadata seo, string natural) => FirstNonEmpty(seo.MetaTitle, natural);

    public static string Description(SeoMetadata seo, string summary, string description) =>
        FirstNonEmpty(seo.MetaDescription, summary, description);

    public static string OgImage(SeoMetadata seo, string coverPath) => FirstNonEmpty(seo.OgImagePath, coverPath);

    public static string OgImageAlt(SeoMetadata seo, string coverAlt, string title) =>
        FirstNonEmpty(seo.OgImageAlt, coverAlt, title);

    private static string FirstNonEmpty(params string[] values) =>
        Array.Find(values, v => !string.IsNullOrWhiteSpace(v)) ?? "";
}
