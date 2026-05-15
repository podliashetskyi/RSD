#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Values;

public partial class ValueEdit(IValueService Service, NavigationManager Nav)
{
    [Parameter] public Guid? Id { get; set; }
    private ValueInput Input { get; set; } = new();
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
        if (existing is null) { Nav.NavigateTo("/admin/values"); return; }
        Input = ValueInput.From(existing);
    }

    private async Task SaveAsync()
    {
        try
        {
            var entity = Input.ToEntity(Id);
            var (ok, error) = await PersistAsync(entity);
            if (!ok) { ErrorMessage = error; return; }
            Nav.NavigateTo("/admin/values");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Save failed: {ex.Message}";
        }
    }

    private async Task<(bool Ok, string Error)> PersistAsync(Value entity)
    {
        if (IsCreate)
        {
            var created = await Service.CreateAsync(entity, CancellationToken.None);
            return (created.Ok, created.Error);
        }
        var updated = await Service.UpdateAsync(entity, CancellationToken.None);
        return (updated.Ok, updated.Error);
    }

    public sealed record class ValueInput
    {
        [Required]
        [StringLength(FieldLimits.Value.Title)]
        public string Title { get; set; } = "";
        [StringLength(FieldLimits.Value.Description)]
        public string Description { get; set; } = "";
        [StringLength(FieldLimits.Value.IconPath)]
        public string IconPath { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";

        public static ValueInput From(Value v) => new()
        {
            Title = v.Title, Description = v.Description, IconPath = v.IconPath,
            Status = v.Status, DisplayOrder = v.DisplayOrder, Slug = v.Slug,
        };

        public Value ToEntity(Guid? id) => new()
        {
            Id = id ?? Guid.NewGuid(), Slug = Slug,
            Title = Title, Description = Description, IconPath = IconPath,
            Status = Status, DisplayOrder = DisplayOrder,
        };
    }
}
