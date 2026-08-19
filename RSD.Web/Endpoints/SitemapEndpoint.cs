using System.Text;
using System.Web;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;
using RSD.Web.Services.Seo;

namespace RSD.Web.Endpoints;

public static class SitemapEndpoint
{
    private const string CacheTag = "sitemap";
    private const int TtlMinutes = 60;

    public static IEndpointRouteBuilder MapSitemap(this IEndpointRouteBuilder app)
    {
        app.MapGet("/sitemap.xml", HandleAsync)
            .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(TtlMinutes)).Tag(CacheTag))
            .AllowAnonymous();
        return app;
    }

    internal static async Task<IResult> HandleAsync(HttpContext http, ISitemapBuilder Builder, IOptions<SeoOptions> Seo)
    {
        var baseUrl = RequestOrigin.Resolve(Seo.Value, http.Request);
        var entries = await Builder.BuildAsync(baseUrl, http.RequestAborted);
        return Results.Text(RenderXml(entries), "application/xml", Encoding.UTF8);
    }

    private static string RenderXml(IReadOnlyList<SitemapEntry> entries)
    {
        var sb = new StringBuilder(entries.Count * 128);
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        foreach (var entry in entries)
        {
            sb.Append("  <url><loc>").Append(HttpUtility.HtmlEncode(entry.Loc)).Append("</loc>");
            sb.Append("<lastmod>").Append(entry.LastMod.ToString("yyyy-MM-ddTHH:mm:ssZ")).Append("</lastmod>");
            sb.AppendLine("</url>");
        }
        sb.AppendLine("</urlset>");
        return sb.ToString();
    }
}
