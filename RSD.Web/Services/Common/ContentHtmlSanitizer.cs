using Ganss.Xss;

namespace RSD.Web.Services.Common;

public sealed class ContentHtmlSanitizer : IContentHtmlSanitizer
{
    private readonly HtmlSanitizer Inner = BuildSanitizer();

    public string Sanitize(string html) => string.IsNullOrWhiteSpace(html) ? "" : Inner.Sanitize(html);

    private static HtmlSanitizer BuildSanitizer()
    {
        var s = new HtmlSanitizer();
        s.AllowedTags.Clear();
        s.AllowedAttributes.Clear();
        s.AllowedCssProperties.Clear();
        s.AllowedSchemes.Clear();

        foreach (var t in AllowedTags) s.AllowedTags.Add(t);
        foreach (var a in AllowedAttributes) s.AllowedAttributes.Add(a);
        foreach (var sc in AllowedSchemes) s.AllowedSchemes.Add(sc);
        return s;
    }

    private static readonly string[] AllowedTags =
    [
        "p", "br", "strong", "b", "em", "i", "u",
        "h2", "h3", "h4",
        "ul", "ol", "li",
        "a", "blockquote", "code", "pre", "span"
    ];

    private static readonly string[] AllowedAttributes =
    [
        "href", "title", "target", "rel", "class"
    ];

    private static readonly string[] AllowedSchemes = ["http", "https", "mailto"];
}
