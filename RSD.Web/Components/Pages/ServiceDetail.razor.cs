#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using RSD.Web.Components.Sections.Article;
using RSD.Web.Services.Content;
using RSD.Web.Services.Preview;
using RSD.Web.Services.Seo;

namespace RSD.Web.Components.Pages;

public partial class ServiceDetail(
    IServiceService Services,
    IHttpContextAccessor Http,
    IPreviewContext PreviewCtx,
    PreviewLink Preview)
{
    [Parameter] public string Slug { get; set; } = "";
    [SupplyParameterFromQuery] public string? Token { get; set; }

    private RSD.Web.Data.Entities.Service? Svc { get; set; }

    private static readonly IReadOnlyList<TocEntry> TocItems = [];

    private string HeroImage => string.IsNullOrEmpty(Svc?.CoverImagePath) ? "images/services/cloud-solutions/hero.png" : Svc!.CoverImagePath;
    private string HeroAlt => string.IsNullOrEmpty(Svc?.CoverImageAlt) ? (Svc?.Title ?? "") : Svc!.CoverImageAlt;
    private string DateText => (Svc?.PublishedAt ?? Svc?.CreatedAt ?? DateTime.UtcNow).ToString("MMMM dd, yyyy");

    private string SeoTitle => Svc is null ? "" : SeoFallbacks.Title(Svc.Seo, Svc.Title);
    private string SeoDescription => Svc is null ? "" : SeoFallbacks.Description(Svc.Seo, Svc.Summary, Svc.Description);
    private string SeoOgImage => Svc is null ? "" : SeoFallbacks.OgImage(Svc.Seo, Svc.CoverImagePath);
    private string SeoOgImageAlt => Svc is null ? "" : SeoFallbacks.OgImageAlt(Svc.Seo, Svc.CoverImageAlt, Svc.Title);
    private string SeoRobots => PreviewCtx.IsPreview ? "noindex" : "";

    protected override async Task OnInitializedAsync()
    {
        if (IsPreviewRequest() && !Preview.Verify("services", Slug, Token))
        {
            NotFound();
            return;
        }
        PreviewCtx.IsPreview = IsPreviewRequest();

        Svc = await Services.GetBySlugAsync(Slug, includeDrafts: PreviewCtx.IsPreview, CancellationToken.None);
        if (Svc is null) NotFound();
    }

    private bool IsPreviewRequest() =>
        Http.HttpContext?.Request.Path.StartsWithSegments("/preview") ?? false;

    private void NotFound()
    {
        var http = Http.HttpContext;
        if (http is not null) http.Response.StatusCode = StatusCodes.Status404NotFound;
    }
}
