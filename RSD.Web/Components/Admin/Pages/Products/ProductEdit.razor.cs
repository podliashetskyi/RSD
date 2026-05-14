#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Products;

public partial class ProductEdit(IProductService Service, NavigationManager Nav, IToastService Toasts) : ComponentBase
{
    [Parameter] public Guid? Id { get; set; }

    private ProductInput Input { get; set; } = new();
    private ProductBodyForm Body { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool SlugIsValid { get; set; } = true;
    private bool IsCreate => Id is null;
    private bool CanSave => SlugIsValid;

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/products"); return; }
        Input = ProductInput.From(existing);
        Body = ProductBodyForm.From(existing.DetailFields);
    }

    private async Task SaveAsync()
    {
        if (!CanSave) { ErrorMessage = "Resolve validation errors before saving."; return; }
        var upsert = Input.ToUpsert(Body.ToEntity());
        var (ok, error) = IsCreate
            ? await CreateAsync(upsert)
            : await UpdateAsync(Id!.Value, upsert);
        if (!ok) { ErrorMessage = error; return; }
        Toasts.Show(IsCreate ? "Product created." : "Product saved.", ToastKind.Success);
        Nav.NavigateTo("/admin/products");
    }

    private async Task<(bool Ok, string Error)> CreateAsync(ProductUpsert upsert)
    {
        var r = await Service.CreateAsync(upsert, CancellationToken.None);
        return (r.Ok, r.Error);
    }

    private async Task<(bool Ok, string Error)> UpdateAsync(Guid id, ProductUpsert upsert)
    {
        var r = await Service.UpdateAsync(id, upsert, CancellationToken.None);
        return (r.Ok, r.Error);
    }

    private void OnSlugChanged(string slug) => Input.Slug = slug;
    private void OnSlugValidityChanged(bool valid) => SlugIsValid = valid;
    private void OnBulletsChanged(List<string> items) => Input.BulletPoints = items;
    private void OnSeoChanged(SeoMetadata seo) => Input.Seo = seo;
    private void OnCoverUploaded(UploadedFile? file) { if (file is not null) Input.CoverImagePath = file.Path; }
    private void ClearCover() => Input.CoverImagePath = "";

    public sealed record class ProductInput
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Price { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> BulletPoints { get; set; } = [];
        public string CoverImagePath { get; set; } = "";
        public string TryForFreeHref { get; set; } = "";
        public string LearnMoreHref { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Draft;
        public SeoMetadata Seo { get; set; } = new();

        public static ProductInput From(Product p) => new()
        {
            Name = p.Name,
            Slug = p.Slug,
            Subtitle = p.Subtitle,
            Price = p.Price,
            Description = p.Description,
            BulletPoints = [.. p.BulletPoints],
            CoverImagePath = p.CoverImagePath,
            TryForFreeHref = p.TryForFreeHref,
            LearnMoreHref = p.LearnMoreHref,
            Status = p.Status,
            Seo = p.Seo
        };

        public ProductUpsert ToUpsert(ProductDetailFields detail) => new(
            Slug, Name, Subtitle, Price, Description, [.. BulletPoints],
            CoverImagePath, TryForFreeHref, LearnMoreHref, Status, Seo, detail);
    }
}
