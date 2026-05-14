using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;
using RSD.Web.Services.Common;

namespace RSD.Web.Services.Content;

internal static class ArticleBodySanitizer
{
    public static ArticleBody Sanitize(ArticleBody body, IContentHtmlSanitizer html) => new()
    {
        Intro = html.Sanitize(body.Intro),
        Blocks = [.. body.Blocks.Select(b => SanitizeBlock(b, html))],
    };

    private static ArticleBlock SanitizeBlock(ArticleBlock block, IContentHtmlSanitizer html) => block switch
    {
        RichTextBlock rt => rt with { Html = html.Sanitize(rt.Html) },
        SubsectionBlock s => s with { Body = html.Sanitize(s.Body) },
        _ => block,
    };
}
