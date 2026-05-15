#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
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
        var entity = Input.ToEntity(Id);
        var (ok, error) = await PersistAsync(entity);
        if (!ok) { ErrorMessage = error; return; }
        Nav.NavigateTo("/admin/testimonials");
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
        [Required] public string Title { get; set; } = "";
        [Required] public string Quote { get; set; } = "";
        [Required] public string AuthorName { get; set; } = "";
        public string AuthorRole { get; set; } = "";
        public string AvatarPath { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        public bool DisplayOnHome { get; set; } = true;
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
