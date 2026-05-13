#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Products;

public partial class ProductList(IProductService Service, IToastService Toasts) : ComponentBase
{
    private List<Product> Items { get; set; } = [];
    private string Search { get; set; } = "";
    private ContentStatus? StatusFilter { get; set; }

    protected override Task OnInitializedAsync() => ReloadAsync();

    private async Task ReloadAsync()
    {
        var list = await Service.ListAsync(new ContentQuery(Status: StatusFilter, Search: Search, PageSize: 200), CancellationToken.None);
        Items = [.. list];
    }

    private async Task OnStatusChangedAsync(ContentStatus? next)
    {
        StatusFilter = next;
        await ReloadAsync();
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await Service.SoftDeleteAsync(id, CancellationToken.None);
        if (result.Ok) Toasts.Show("Product deleted.", ToastKind.Success);
        else Toasts.Show(result.Error, ToastKind.Error);
        await ReloadAsync();
    }
}
