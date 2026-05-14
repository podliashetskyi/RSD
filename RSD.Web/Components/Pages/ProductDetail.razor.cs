#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Preview;

namespace RSD.Web.Components.Pages;

public partial class ProductDetail(
    IProductService Products,
    IHttpContextAccessor Http,
    IPreviewContext PreviewCtx,
    PreviewLink Preview)
{
    [Parameter] public string Slug { get; set; } = "";
    [SupplyParameterFromQuery] public string? Token { get; set; }

    private Product? Product { get; set; }

    private string HeroImage => string.IsNullOrEmpty(Product?.CoverImagePath) ? "images/products/nexacrm/hero-dashboard.png" : Product!.CoverImagePath;

    protected override async Task OnInitializedAsync()
    {
        if (IsPreviewRequest() && !Preview.Verify("products", Slug, Token))
        {
            NotFound();
            return;
        }
        PreviewCtx.IsPreview = IsPreviewRequest();

        Product = await Products.GetBySlugAsync(Slug, includeDrafts: PreviewCtx.IsPreview, CancellationToken.None);
        if (Product is null) NotFound();
    }

    private bool IsPreviewRequest() =>
        Http.HttpContext?.Request.Path.StartsWithSegments("/preview") ?? false;

    private void NotFound()
    {
        var http = Http.HttpContext;
        if (http is not null) http.Response.StatusCode = StatusCodes.Status404NotFound;
    }
}
