#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Components.Admin.Shared.Blocks;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Preview;

namespace RSD.Web.Components.Admin.Pages.Services;

public partial class ServiceEdit(
    IServiceService Service,
    NavigationManager Nav,
    IToastService Toasts,
    PreviewLink Preview) : ComponentBase
{
    [Parameter] public Guid? Id { get; set; }

    private ServiceInput Input { get; set; } = new();
    private ArticleBodyForm Body { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool SlugIsValid { get; set; } = true;
    private string LoadedSlug { get; set; } = "";
    private bool IsCreate => Id is null;
    private bool CanSave => SlugIsValid;
    private string PreviewUrl => string.IsNullOrEmpty(LoadedSlug) ? "" : Preview.Build("services", LoadedSlug);

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/services"); return; }
        Input = ServiceInput.From(existing);
        Body = ArticleBodyForm.From(existing.BodyBlocks);
        LoadedSlug = existing.Slug;
    }

    private async Task SaveAsync()
    {
        if (!CanSave) { ErrorMessage = "Resolve validation errors before saving."; return; }
        try
        {
            var upsert = Input.ToUpsert(Body.ToEntity());
            if (IsCreate) await HandleCreateAsync(upsert);
            else await HandleUpdateAsync(Id!.Value, upsert);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Save failed: {ex.Message}";
        }
    }

    private async Task HandleCreateAsync(ServiceUpsert upsert)
    {
        var created = await Service.CreateAsync(upsert, CancellationToken.None);
        if (!created.Ok) { ErrorMessage = created.Error; return; }
        Toasts.Show("Service created.", ToastKind.Success);
        Nav.NavigateTo($"/admin/services/{created.Value}");
    }

    private async Task HandleUpdateAsync(Guid id, ServiceUpsert upsert)
    {
        var updated = await Service.UpdateAsync(id, upsert, CancellationToken.None);
        if (!updated.Ok) { ErrorMessage = updated.Error; return; }
        Toasts.Show("Service saved.", ToastKind.Success);
        Nav.NavigateTo("/admin/services");
    }

    private void OnSlugChanged(string slug) => Input.Slug = slug;
    private void OnSlugValidityChanged(bool valid) => SlugIsValid = valid;
    private void OnBulletsChanged(List<string> items) => Input.BulletPoints = items;
    private void OnSeoChanged(SeoMetadata seo) => Input.Seo = seo;
    private void OnCoverUploaded(UploadedFile? file) { if (file is not null) Input.CoverImagePath = file.Path; }
    private void OnCoverAltChanged(string alt) => Input.CoverImageAlt = alt;
    private void ClearCover() => Input.CoverImagePath = "";

    public sealed record class ServiceInput
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(FieldLimits.Service.Title)]
        public string Title { get; set; } = "";
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";
        [StringLength(FieldLimits.Service.Summary)]
        public string Summary { get; set; } = "";
        [StringLength(FieldLimits.Service.Description)]
        public string Description { get; set; } = "";
        public List<string> BulletPoints { get; set; } = [];
        [StringLength(FieldLimits.Service.CoverImagePath)]
        public string CoverImagePath { get; set; } = "";
        [StringLength(FieldLimits.Service.CoverImageAlt)]
        public string CoverImageAlt { get; set; } = "";
        [StringLength(FieldLimits.Service.DetailsHref)]
        public string DetailsHref { get; set; } = "";
        [StringLength(FieldLimits.Service.Intro)]
        public string Intro { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Draft;
        public SeoMetadata Seo { get; set; } = new();

        public static ServiceInput From(RSD.Web.Data.Entities.Service s) => new()
        {
            Title = s.Title,
            Slug = s.Slug,
            Summary = s.Summary,
            Description = s.Description,
            BulletPoints = [.. s.BulletPoints],
            CoverImagePath = s.CoverImagePath,
            CoverImageAlt = s.CoverImageAlt,
            DetailsHref = s.DetailsHref,
            Intro = s.Intro,
            Status = s.Status,
            Seo = s.Seo
        };

        public ServiceUpsert ToUpsert(ArticleBody body) => new(
            Slug, Title, Summary, Description, [.. BulletPoints], CoverImagePath, CoverImageAlt, DetailsHref, Intro, Status, Seo, body);
    }
}
