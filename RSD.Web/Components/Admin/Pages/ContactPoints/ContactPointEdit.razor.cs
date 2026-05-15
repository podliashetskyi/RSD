#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.ContactPoints;

public partial class ContactPointEdit(IContactPointService Service, NavigationManager Nav)
{
    [Parameter] public Guid? Id { get; set; }
    [SupplyParameterFromForm] private ContactInput Input { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool IsCreate => Id is null;

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/contact-points"); return; }
        Input = ContactInput.From(existing);
    }

    private async Task SaveAsync()
    {
        var entity = Input.ToEntity(Id);
        var (ok, error) = await PersistAsync(entity);
        if (!ok) { ErrorMessage = error; return; }
        Nav.NavigateTo("/admin/contact-points");
    }

    private async Task<(bool Ok, string Error)> PersistAsync(ContactPoint entity)
    {
        if (IsCreate)
        {
            var created = await Service.CreateAsync(entity, CancellationToken.None);
            return (created.Ok, created.Error);
        }
        var updated = await Service.UpdateAsync(entity, CancellationToken.None);
        return (updated.Ok, updated.Error);
    }

    public sealed record class ContactInput
    {
        [Required]
        [StringLength(FieldLimits.ContactPoint.Label)]
        public string Label { get; set; } = "";
        public string LinesText { get; set; } = "";
        public bool IsLink { get; set; }
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";

        public static ContactInput From(ContactPoint c) => new()
        {
            Label = c.Label,
            LinesText = string.Join("\n", c.Lines),
            IsLink = c.IsLink,
            Status = c.Status, DisplayOrder = c.DisplayOrder, Slug = c.Slug,
        };

        public ContactPoint ToEntity(Guid? id) => new()
        {
            Id = id ?? Guid.NewGuid(), Slug = Slug,
            Label = Label,
            Lines = ParseLines(LinesText),
            IsLink = IsLink,
            Status = Status, DisplayOrder = DisplayOrder,
        };

        private static List<string> ParseLines(string text) =>
            text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}
