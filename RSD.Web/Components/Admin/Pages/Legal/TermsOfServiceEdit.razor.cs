#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Legal;

public partial class TermsOfServiceEdit(
    ITermsOfServiceService Service,
        IToastService Toasts) : ComponentBase
{
    private TermsOfServiceInput Input { get; set; } = new();
    private Guid EntityId { get; set; }
    private string EntitySlug { get; set; } = "";
    private string ErrorMessage { get; set; } = "";
    private bool Loaded { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var rows = await Service.ListAsync(new ContentQuery(PageSize: 1, IncludeDeleted: true), CancellationToken.None);
        var existing = rows.FirstOrDefault();
        if (existing is null) { ErrorMessage = "Terms of Service row not found. Run the seeder."; return; }
        EntityId = existing.Id;
        EntitySlug = existing.Slug;
        Input = TermsOfServiceInput.From(existing);
        Loaded = true;
    }

    private async Task SaveAsync()
    {
        try
        {
            var entity = Input.ToEntity(EntityId, EntitySlug);
            var result = await Service.UpdateAsync(entity, CancellationToken.None);
            if (!result.Ok) { ErrorMessage = result.Error; return; }
            Toasts.Show("Terms of Service saved.", ToastKind.Success);
            ErrorMessage = "";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Save failed: {ex.Message}";
        }
    }

    private void OnSeoChanged(SeoMetadata seo) => Input.Seo = seo;

    public sealed record class TermsOfServiceInput
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(FieldLimits.TermsOfService.Title)]
        public string Title { get; set; } = "Terms of Service";

        public DateOnly LastUpdatedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public string BodyHtml { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public SeoMetadata Seo { get; set; } = new();

        public static TermsOfServiceInput From(TermsOfService t) => new()
        {
            Title = t.Title,
            LastUpdatedAt = t.LastUpdatedAt,
            BodyHtml = t.BodyHtml,
            Status = t.Status,
            Seo = t.Seo,
        };

        public TermsOfService ToEntity(Guid id, string slug) => new()
        {
            Id = id,
            Slug = slug,
            Title = Title,
            LastUpdatedAt = LastUpdatedAt,
            BodyHtml = BodyHtml,
            Status = Status,
            Seo = Seo,
        };
    }
}
