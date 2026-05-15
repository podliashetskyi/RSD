#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Preview;

namespace RSD.Web.Components.Admin.Pages.Cases;

public partial class CaseEdit(
    ICaseService Service,
    NavigationManager Nav,
    IToastService Toasts,
    PreviewLink Preview) : ComponentBase
{
    [Parameter] public Guid? Id { get; set; }

    private CaseInput Input { get; set; } = new();
    private CaseBodyForm Body { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool SlugIsValid { get; set; } = true;
    private string LoadedSlug { get; set; } = "";
    private bool IsCreate => Id is null;
    private bool CanSave => SlugIsValid;
    private string PreviewUrl => string.IsNullOrEmpty(LoadedSlug) ? "" : Preview.Build("cases", LoadedSlug);

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
        LoadedSlug = existing.Slug;
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
    private void OnCoverAltChanged(string alt) => Input.CoverImageAlt = alt;
    private void ClearCover() => Input.CoverImagePath = "";

    public sealed record class CaseInput
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(FieldLimits.Case.Name)]
        public string Name { get; set; } = "";
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";
        [StringLength(FieldLimits.Case.Summary)]
        public string Summary { get; set; } = "";
        [StringLength(FieldLimits.Case.Industry)]
        public string Industry { get; set; } = "";
        [StringLength(FieldLimits.Case.Description)]
        public string Description { get; set; } = "";
        [StringLength(FieldLimits.Case.CoverImagePath)]
        public string CoverImagePath { get; set; } = "";
        [StringLength(FieldLimits.Case.CoverImageAlt)]
        public string CoverImageAlt { get; set; } = "";
        public List<string> TechTags { get; set; } = [];
        public ContentStatus Status { get; set; } = ContentStatus.Draft;
        public SeoMetadata Seo { get; set; } = new();

        public static CaseInput From(Case c) => new()
        {
            Name = c.Name,
            Slug = c.Slug,
            Summary = c.Summary,
            Industry = c.Industry,
            Description = c.Description,
            CoverImagePath = c.CoverImagePath,
            CoverImageAlt = c.CoverImageAlt,
            TechTags = [.. c.TechTags],
            Status = c.Status,
            Seo = c.Seo
        };

        public CaseUpsert ToUpsert(CaseDetailFields detail) => new(
            Slug, Name, Summary, Industry, Description, CoverImagePath, CoverImageAlt, [.. TechTags], Status, Seo, detail);
    }
}
