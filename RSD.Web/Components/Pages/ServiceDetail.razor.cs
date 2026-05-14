#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using RSD.Web.Components.Sections.Article;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Pages;

public partial class ServiceDetail(
    IServiceService Services,
    IHttpContextAccessor Http)
{
    [Parameter] public string Slug { get; set; } = "";

    private RSD.Web.Data.Entities.Service? Svc { get; set; }

    private static readonly IReadOnlyList<TocEntry> TocItems = [];

    private string HeroImage => string.IsNullOrEmpty(Svc?.CoverImagePath) ? "images/services/cloud-solutions/hero.png" : Svc!.CoverImagePath;
    private string DateText => (Svc?.PublishedAt ?? Svc?.CreatedAt ?? DateTime.UtcNow).ToString("MMMM dd, yyyy");

    protected override async Task OnInitializedAsync()
    {
        Svc = await Services.GetBySlugAsync(Slug, includeDrafts: false, CancellationToken.None);
        if (Svc is null)
        {
            var http = Http.HttpContext;
            if (http is not null) http.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}
