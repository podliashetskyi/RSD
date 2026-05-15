#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.MessengerLinks;

public partial class MessengerLinkEdit(IMessengerLinkService Service, NavigationManager Nav)
{
    [Parameter] public Guid? Id { get; set; }
    private MessengerInput Input { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool IsCreate => Id is null;

    private void OnLargeIconUploaded(UploadedFile? file)
    {
        if (file is not null) Input.LargeIconPath = file.Path;
    }

    private void OnSmallIconUploaded(UploadedFile? file)
    {
        if (file is not null) Input.SmallIconPath = file.Path;
    }

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/messenger-links"); return; }
        Input = MessengerInput.From(existing);
    }

    private async Task SaveAsync()
    {
        var entity = Input.ToEntity(Id);
        var (ok, error) = await PersistAsync(entity);
        if (!ok) { ErrorMessage = error; return; }
        Nav.NavigateTo("/admin/messenger-links");
    }

    private async Task<(bool Ok, string Error)> PersistAsync(MessengerLink entity)
    {
        if (IsCreate)
        {
            var created = await Service.CreateAsync(entity, CancellationToken.None);
            return (created.Ok, created.Error);
        }
        var updated = await Service.UpdateAsync(entity, CancellationToken.None);
        return (updated.Ok, updated.Error);
    }

    public sealed record class MessengerInput
    {
        [Required]
        [StringLength(FieldLimits.MessengerLink.Label)]
        public string Label { get; set; } = "";
        [StringLength(FieldLimits.MessengerLink.LargeIconPath)]
        public string LargeIconPath { get; set; } = "";
        [StringLength(FieldLimits.MessengerLink.SmallIconPath)]
        public string SmallIconPath { get; set; } = "";
        [StringLength(FieldLimits.MessengerLink.BgColor)]
        public string BgColor { get; set; } = "";
        [StringLength(FieldLimits.MessengerLink.Href)]
        public string Href { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";

        public static MessengerInput From(MessengerLink m) => new()
        {
            Label = m.Label, LargeIconPath = m.LargeIconPath, SmallIconPath = m.SmallIconPath,
            BgColor = m.BgColor, Href = m.Href,
            Status = m.Status, DisplayOrder = m.DisplayOrder, Slug = m.Slug,
        };

        public MessengerLink ToEntity(Guid? id) => new()
        {
            Id = id ?? Guid.NewGuid(), Slug = Slug,
            Label = Label, LargeIconPath = LargeIconPath, SmallIconPath = SmallIconPath,
            BgColor = BgColor, Href = Href,
            Status = Status, DisplayOrder = DisplayOrder,
        };
    }
}
