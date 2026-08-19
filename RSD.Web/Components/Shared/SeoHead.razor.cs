#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using RSD.Web.Services.Seo;

namespace RSD.Web.Components.Shared;

public partial class SeoHead(IOptions<SeoOptions> Seo, IHttpContextAccessor HttpAccessor)
{
    private const string DefaultOgAsset = "images/og-default.png";

    [Parameter, EditorRequired] public string Title { get; set; } = "";
    [Parameter] public string Description { get; set; } = "";
    [Parameter] public string OgImagePath { get; set; } = "";
    [Parameter] public string OgImageAlt { get; set; } = "";
    [Parameter] public string Type { get; set; } = "website";
    [Parameter] public string Robots { get; set; } = "";
    [Parameter] public string CanonicalPath { get; set; } = "";

    private bool HasDescription => Description.Length > 0;
    private bool HasOgImageAlt => OgImageAlt.Length > 0;
    private bool HasRobots => Robots.Length > 0;

    private string CanonicalUrl => AbsoluteUrl.Compose(OriginValue, CanonicalPath.Length > 0 ? CanonicalPath : RequestPath);

    private string OgImageUrl => AbsoluteUrl.Compose(OriginValue, OgImagePath.Length > 0 ? OgImagePath : DefaultOgAsset);

    // Canonical deliberately ignores the query string: filtered/searched listing variants
    // must consolidate onto the clean listing URL.
    private string RequestPath => HttpAccessor.HttpContext?.Request.Path.Value ?? "/";

    private string OriginValue
    {
        get
        {
            var request = HttpAccessor.HttpContext?.Request;
            return request is null ? new Origin(Seo.Value.BaseUrl).Value : RequestOrigin.Resolve(Seo.Value, request);
        }
    }
}
