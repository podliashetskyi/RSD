#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Testimonials;

public partial class TestimonialEdit(ITestimonialService Service, NavigationManager Nav)
{
    [Parameter] public Guid? Id { get; set; }
    private TestimonialInput Input { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool IsCreate => Id is null;

    private void OnAvatarUploaded(UploadedFile? file)
    {
        if (file is not null) Input.AvatarPath = file.Path;
    }

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/testimonials"); return; }
        Input = TestimonialInput.From(existing);
    }

    private async Task SaveAsync()
    {
        try
        {
            var entity = Input.ToEntity(Id);
            var (ok, error) = await PersistAsync(entity);
            if (!ok) { ErrorMessage = error; return; }
            Nav.NavigateTo("/admin/testimonials");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Save failed: {ex.Message}";
        }
    }

    private async Task<(bool Ok, string Error)> PersistAsync(Testimonial entity)
    {
        if (IsCreate)
        {
            var created = await Service.CreateAsync(entity, CancellationToken.None);
            return (created.Ok, created.Error);
        }
        var updated = await Service.UpdateAsync(entity, CancellationToken.None);
        return (updated.Ok, updated.Error);
    }

    public sealed record class TestimonialInput
    {
        [Required]
        [StringLength(FieldLimits.Testimonial.Title)]
        public string Title { get; set; } = "";
        [Required]
        [StringLength(FieldLimits.Testimonial.Quote)]
        public string Quote { get; set; } = "";
        [Required]
        [StringLength(FieldLimits.Testimonial.AuthorName)]
        public string AuthorName { get; set; } = "";
        [StringLength(FieldLimits.Testimonial.AuthorRole)]
        public string AuthorRole { get; set; } = "";
        [StringLength(FieldLimits.Testimonial.AvatarPath)]
        public string AvatarPath { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        public bool DisplayOnHome { get; set; } = true;
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";

        public static TestimonialInput From(Testimonial t) => new()
        {
            Title = t.Title,
            Quote = t.Quote,
            AuthorName = t.AuthorName,
            AuthorRole = t.AuthorRole,
            AvatarPath = t.AvatarPath,
            Status = t.Status,
            DisplayOrder = t.DisplayOrder,
            DisplayOnHome = t.DisplayOnHome,
            Slug = t.Slug,
        };

        public Testimonial ToEntity(Guid? id) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Slug = Slug,
            Title = Title,
            Quote = Quote,
            AuthorName = AuthorName,
            AuthorRole = AuthorRole,
            AvatarPath = AvatarPath,
            Status = Status,
            DisplayOrder = DisplayOrder,
            DisplayOnHome = DisplayOnHome,
        };
    }
}
