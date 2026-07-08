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
    private ContactInput Input { get; set; } = new();
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
        if (existing is null) { Nav.NavigateTo("/admin/contact-points"); return; }
        Input = ContactInput.From(existing);
    }

    private async Task SaveAsync()
    {
        try
        {
            var entity = Input.ToEntity(Id);
            var (ok, error) = await PersistAsync(entity);
            if (!ok) { ErrorMessage = error; return; }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Save failed: {ex.Message}";
            return;
        }
        // Outside the try: in static SSR, NavigateTo signals the redirect by throwing
        // NavigationException, which must reach the framework — never a catch block.
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

    public sealed record class ContactInput : IValidatableObject
    {
        [Required]
        [StringLength(FieldLimits.ContactPoint.Label)]
        public string Label { get; set; } = "";
        public string LinesText { get; set; } = "";
        [Display(Name = "Link")]
        [StringLength(FieldLimits.ContactPoint.Href)]
        public string Href { get; set; } = "";
        [StringLength(FieldLimits.ContactPoint.IconPath)]
        public string IconPath { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";

        public static ContactInput From(ContactPoint c) => new()
        {
            Label = c.Label,
            LinesText = string.Join("\n", c.Lines),
            Href = c.Href,
            IconPath = c.IconPath,
            Status = c.Status, DisplayOrder = c.DisplayOrder, Slug = c.Slug,
        };

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (LinkHrefValidator.IsValidContactHref(Href)) yield break;
            yield return new ValidationResult(LinkHrefValidator.ContactHrefMessage, [nameof(Href)]);
        }

        public ContactPoint ToEntity(Guid? id) => new()
        {
            Id = id ?? Guid.NewGuid(), Slug = Slug,
            Label = Label,
            Lines = ParseLines(LinesText),
            Href = Href,
            IconPath = IconPath,
            IsLink = !string.IsNullOrWhiteSpace(Href),
            Status = Status, DisplayOrder = DisplayOrder,
        };

        private static List<string> ParseLines(string text) =>
            text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}
