#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.SocialLinks;

public partial class SocialLinkEdit(ISocialLinkService Service, NavigationManager Nav)
{
    [Parameter] public Guid? Id { get; set; }
    private SocialInput Input { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool IsCreate => Id is null;

    private void OnIconUploaded(UploadedFile? file)
    {
        if (file is not null) Input.IconPath = file.Path;
    }

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/social-links"); return; }
        Input = SocialInput.From(existing);
    }

    private async Task SaveAsync()
    {
        var entity = Input.ToEntity(Id);
        var (ok, error) = await PersistAsync(entity);
        if (!ok) { ErrorMessage = error; return; }
        Nav.NavigateTo("/admin/social-links");
    }

    private async Task<(bool Ok, string Error)> PersistAsync(SocialLink entity)
    {
        if (IsCreate)
        {
            var created = await Service.CreateAsync(entity, CancellationToken.None);
            return (created.Ok, created.Error);
        }
        var updated = await Service.UpdateAsync(entity, CancellationToken.None);
        return (updated.Ok, updated.Error);
    }

    public sealed record class SocialInput
    {
        [Required]
        [StringLength(FieldLimits.SocialLink.Label)]
        public string Label { get; set; } = "";
        [StringLength(FieldLimits.SocialLink.IconPath)]
        public string IconPath { get; set; } = "";
        [StringLength(FieldLimits.SocialLink.Href)]
        public string Href { get; set; } = "";
        public SocialLinkScope Scope { get; set; } = SocialLinkScope.Footer;
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";

        public static SocialInput From(SocialLink s) => new()
        {
            Label = s.Label, IconPath = s.IconPath, Href = s.Href, Scope = s.Scope,
            Status = s.Status, DisplayOrder = s.DisplayOrder, Slug = s.Slug,
        };

        public SocialLink ToEntity(Guid? id) => new()
        {
            Id = id ?? Guid.NewGuid(), Slug = Slug,
            Label = Label, IconPath = IconPath, Href = Href, Scope = Scope,
            Status = Status, DisplayOrder = DisplayOrder,
        };
    }
}
