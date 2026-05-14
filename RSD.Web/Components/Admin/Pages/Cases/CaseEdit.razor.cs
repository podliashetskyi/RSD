#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Cases;

public partial class CaseEdit(ICaseService Service, NavigationManager Nav, IToastService Toasts) : ComponentBase
{
    [Parameter] public Guid? Id { get; set; }

    private CaseInput Input { get; set; } = new();
    private CaseBodyForm Body { get; set; } = new();
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
        if (existing is null) { Nav.NavigateTo("/admin/cases"); return; }
        Input = CaseInput.From(existing);
        Body = CaseBodyForm.From(existing.DetailFields);
    }

    private async Task SaveAsync()
    {
        if (!CanSave) { ErrorMessage = "Resolve validation errors before saving."; return; }
        var upsert = Input.ToUpsert(Body.ToEntity());
        var (ok, error) = IsCreate
            ? await CreateAsync(upsert)
            : await UpdateAsync(Id!.Value, upsert);
        if (!ok) { ErrorMessage = error; return; }
        Toasts.Show(IsCreate ? "Case created." : "Case saved.", ToastKind.Success);
        Nav.NavigateTo("/admin/cases");
    }

    private async Task<(bool Ok, string Error)> CreateAsync(CaseUpsert upsert)
    {
        var r = await Service.CreateAsync(upsert, CancellationToken.None);
        return (r.Ok, r.Error);
    }

    private async Task<(bool Ok, string Error)> UpdateAsync(Guid id, CaseUpsert upsert)
    {
        var r = await Service.UpdateAsync(id, upsert, CancellationToken.None);
        return (r.Ok, r.Error);
    }

    private void OnSlugChanged(string slug) => Input.Slug = slug;
    private void OnSlugValidityChanged(bool valid) => SlugIsValid = valid;
    private void OnTechTagsChanged(List<string> tags) => Input.TechTags = tags;
    private void OnSeoChanged(SeoMetadata seo) => Input.Seo = seo;
    private void OnCoverUploaded(UploadedFile? file) { if (file is not null) Input.CoverImagePath = file.Path; }
    private void ClearCover() => Input.CoverImagePath = "";

    public sealed record class CaseInput
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Industry { get; set; } = "";
        public string Description { get; set; } = "";
        public string CoverImagePath { get; set; } = "";
        public List<string> TechTags { get; set; } = [];
        public ContentStatus Status { get; set; } = ContentStatus.Draft;
        public SeoMetadata Seo { get; set; } = new();

        public static CaseInput From(Case c) => new()
        {
            Name = c.Name,
            Slug = c.Slug,
            Industry = c.Industry,
            Description = c.Description,
            CoverImagePath = c.CoverImagePath,
            TechTags = [.. c.TechTags],
            Status = c.Status,
            Seo = c.Seo
        };

        public CaseUpsert ToUpsert(CaseDetailFields detail) => new(
            Slug, Name, Industry, Description, CoverImagePath, [.. TechTags], Status, Seo, detail);
    }
}
