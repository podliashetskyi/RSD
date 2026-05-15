#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Tech;

public partial class TechStackItemEdit(ITechStackItemService Service, NavigationManager Nav)
{
    [Parameter] public Guid? Id { get; set; }
    private TechInput Input { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool IsCreate => Id is null;

    private void OnLogoUploaded(UploadedFile? file)
    {
        if (file is not null) Input.LogoPath = file.Path;
    }

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/tech"); return; }
        Input = TechInput.From(existing);
    }

    private async Task SaveAsync()
    {
        var entity = Input.ToEntity(Id);
        var (ok, error) = await PersistAsync(entity);
        if (!ok) { ErrorMessage = error; return; }
        Nav.NavigateTo("/admin/tech");
    }

    private async Task<(bool Ok, string Error)> PersistAsync(TechStackItem entity)
    {
        if (IsCreate)
        {
            var created = await Service.CreateAsync(entity, CancellationToken.None);
            return (created.Ok, created.Error);
        }
        var updated = await Service.UpdateAsync(entity, CancellationToken.None);
        return (updated.Ok, updated.Error);
    }

    public sealed record class TechInput
    {
        [Required] public string Label { get; set; } = "";
        public string LogoPath { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        public string Slug { get; set; } = "";

        public static TechInput From(TechStackItem t) => new()
        {
            Label = t.Label, LogoPath = t.LogoPath,
            Status = t.Status, DisplayOrder = t.DisplayOrder, Slug = t.Slug,
        };

        public TechStackItem ToEntity(Guid? id) => new()
        {
            Id = id ?? Guid.NewGuid(), Slug = Slug,
            Label = Label, LogoPath = LogoPath,
            Status = Status, DisplayOrder = DisplayOrder,
        };
    }
}
