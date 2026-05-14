#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Components.Admin.Shared.Blocks;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Services;

public partial class ServiceEdit(IServiceService Service, NavigationManager Nav, IToastService Toasts) : ComponentBase
{
    [Parameter] public Guid? Id { get; set; }

    private ServiceInput Input { get; set; } = new();
    private ArticleBodyForm Body { get; set; } = new();
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
        if (existing is null) { Nav.NavigateTo("/admin/services"); return; }
        Input = ServiceInput.From(existing);
        Body = ArticleBodyForm.From(existing.BodyBlocks);
    }

    private async Task SaveAsync()
    {
        if (!CanSave) { ErrorMessage = "Resolve validation errors before saving."; return; }
        var upsert = Input.ToUpsert(Body.ToEntity());
        var (ok, error) = IsCreate
            ? await CreateAsync(upsert)
            : await UpdateAsync(Id!.Value, upsert);
        if (!ok) { ErrorMessage = error; return; }
        Toasts.Show(IsCreate ? "Service created." : "Service saved.", ToastKind.Success);
        Nav.NavigateTo("/admin/services");
    }

    private async Task<(bool Ok, string Error)> CreateAsync(ServiceUpsert upsert)
    {
        var r = await Service.CreateAsync(upsert, CancellationToken.None);
        return (r.Ok, r.Error);
    }

    private async Task<(bool Ok, string Error)> UpdateAsync(Guid id, ServiceUpsert upsert)
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

    public sealed record class ServiceInput
    {
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> BulletPoints { get; set; } = [];
        public string CoverImagePath { get; set; } = "";
        public string DetailsHref { get; set; } = "";
        public string Intro { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Draft;
        public SeoMetadata Seo { get; set; } = new();

        public static ServiceInput From(RSD.Web.Data.Entities.Service s) => new()
        {
            Title = s.Title,
            Slug = s.Slug,
            Description = s.Description,
            BulletPoints = [.. s.BulletPoints],
            CoverImagePath = s.CoverImagePath,
            DetailsHref = s.DetailsHref,
            Intro = s.Intro,
            Status = s.Status,
            Seo = s.Seo
        };

        public ServiceUpsert ToUpsert(ArticleBody body) => new(
            Slug, Title, Description, [.. BulletPoints], CoverImagePath, DetailsHref, Intro, Status, Seo, body);
    }
}
