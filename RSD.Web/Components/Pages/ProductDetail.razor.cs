#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Pages;

public partial class ProductDetail(
    IProductService Products,
    IHttpContextAccessor Http)
{
    [Parameter] public string Slug { get; set; } = "";

    private Product? Product { get; set; }

    private string HeroImage => string.IsNullOrEmpty(Product?.CoverImagePath) ? "images/products/nexacrm/hero-dashboard.png" : Product!.CoverImagePath;

    protected override async Task OnInitializedAsync()
    {
        Product = await Products.GetBySlugAsync(Slug, includeDrafts: false, CancellationToken.None);
        if (Product is null)
        {
            var http = Http.HttpContext;
            if (http is not null) http.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}
