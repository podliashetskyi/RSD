#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Pages;

public partial class CaseDetail(
    ICaseService Cases,
    IHttpContextAccessor Http)
{
    [Parameter] public string Slug { get; set; } = "";

    private Case? Case { get; set; }

    private string HeroImage => string.IsNullOrEmpty(Case?.CoverImagePath) ? "images/cases/healthcare-plus/hero.png" : Case!.CoverImagePath;

    protected override async Task OnInitializedAsync()
    {
        Case = await Cases.GetBySlugAsync(Slug, includeDrafts: false, CancellationToken.None);
        if (Case is null)
        {
            var http = Http.HttpContext;
            if (http is not null) http.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}
