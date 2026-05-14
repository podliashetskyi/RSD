#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Preview;

namespace RSD.Web.Components.Pages;

public partial class CaseDetail(
    ICaseService Cases,
    IHttpContextAccessor Http,
    IPreviewContext PreviewCtx,
    PreviewLink Preview)
{
    [Parameter] public string Slug { get; set; } = "";
    [SupplyParameterFromQuery] public string? Token { get; set; }

    private Case? Case { get; set; }

    private string HeroImage => string.IsNullOrEmpty(Case?.CoverImagePath) ? "images/cases/healthcare-plus/hero.png" : Case!.CoverImagePath;

    protected override async Task OnInitializedAsync()
    {
        if (IsPreviewRequest() && !Preview.Verify("cases", Slug, Token))
        {
            NotFound();
            return;
        }
        PreviewCtx.IsPreview = IsPreviewRequest();

        Case = await Cases.GetBySlugAsync(Slug, includeDrafts: PreviewCtx.IsPreview, CancellationToken.None);
        if (Case is null) NotFound();
    }

    private bool IsPreviewRequest() =>
        Http.HttpContext?.Request.Path.StartsWithSegments("/preview") ?? false;

    private void NotFound()
    {
        var http = Http.HttpContext;
        if (http is not null) http.Response.StatusCode = StatusCodes.Status404NotFound;
    }
}
