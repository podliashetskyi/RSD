#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Partners;

public partial class PartnerEdit(IPartnerService Service, NavigationManager Nav)
{
    [Parameter] public Guid? Id { get; set; }
    private PartnerInput Input { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool IsCreate => Id is null;

    private void OnPhotoUploaded(UploadedFile? file)
    {
        if (file is not null) Input.PhotoPath = file.Path;
    }

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/partners"); return; }
        Input = PartnerInput.From(existing);
    }

    private async Task SaveAsync()
    {
        var entity = Input.ToEntity(Id);
        var (ok, error) = await PersistAsync(entity);
        if (!ok) { ErrorMessage = error; return; }
        Nav.NavigateTo("/admin/partners");
    }

    private async Task<(bool Ok, string Error)> PersistAsync(Partner entity)
    {
        if (IsCreate)
        {
            var created = await Service.CreateAsync(entity, CancellationToken.None);
            return (created.Ok, created.Error);
        }
        var updated = await Service.UpdateAsync(entity, CancellationToken.None);
        return (updated.Ok, updated.Error);
    }

    public sealed record class PartnerInput
    {
        [Required] public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public string PhotoPath { get; set; } = "";
        public string ContactHref { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        public string Slug { get; set; } = "";

        public static PartnerInput From(Partner p) => new()
        {
            Name = p.Name, Role = p.Role, PhotoPath = p.PhotoPath, ContactHref = p.ContactHref,
            Status = p.Status, DisplayOrder = p.DisplayOrder, Slug = p.Slug,
        };

        public Partner ToEntity(Guid? id) => new()
        {
            Id = id ?? Guid.NewGuid(), Slug = Slug,
            Name = Name, Role = Role, PhotoPath = PhotoPath, ContactHref = ContactHref,
            Status = Status, DisplayOrder = DisplayOrder,
        };
    }
}
