#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Preview;
using RSD.Web.Services.Seo;

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
    private string HeroAlt => string.IsNullOrEmpty(Product?.CoverImageAlt) ? (Product?.Name ?? "") : Product!.CoverImageAlt;

    private string SeoTitle => Product is null ? "" : SeoFallbacks.Title(Product.Seo, Product.Name);
    private string SeoDescription => Product is null ? "" : SeoFallbacks.Description(Product.Seo, Product.Summary, Product.Description);
    private string SeoOgImage => Product is null ? "" : SeoFallbacks.OgImage(Product.Seo, Product.CoverImagePath);
    private string SeoOgImageAlt => Product is null ? "" : SeoFallbacks.OgImageAlt(Product.Seo, Product.CoverImageAlt, Product.Name);
    private string SeoRobots => PreviewCtx.IsPreview ? "noindex" : "";

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
