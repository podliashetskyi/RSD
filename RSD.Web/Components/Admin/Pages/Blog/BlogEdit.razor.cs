#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Components.Admin.Shared.Blocks;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Blog;

public partial class BlogEdit(
    IBlogService Service,
    NavigationManager Nav,
    IToastService Toasts) : ComponentBase
{
    [Parameter] public Guid? Id { get; set; }

    private BlogPostInput Input { get; set; } = new();
    private ArticleBodyForm Body { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool SlugIsValid { get; set; } = true;
    private bool IsCreate => Id is null;
    private bool CanSave => SlugIsValid;

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/blog"); return; }
        Input = BlogPostInput.From(existing);
        Body = ArticleBodyForm.From(existing.BodyBlocks);
    }

    private async Task SaveAsync()
    {
        if (!CanSave) { ErrorMessage = "Resolve validation errors before saving."; return; }
        var upsert = Input.ToUpsert(Body.ToEntity());
        var (ok, error) = IsCreate
            ? await CreateAsync(upsert)
            : await UpdateAsync(Id!.Value, upsert);
        if (!ok) { ErrorMessage = error; return; }
        Toasts.Show(IsCreate ? "Post created." : "Post saved.", ToastKind.Success);
        Nav.NavigateTo("/admin/blog");
    }

    private async Task<(bool Ok, string Error)> CreateAsync(BlogPostUpsert upsert)
    {
        var r = await Service.CreateAsync(upsert, CancellationToken.None);
        return (r.Ok, r.Error);
    }

    private async Task<(bool Ok, string Error)> UpdateAsync(Guid id, BlogPostUpsert upsert)
    {
        var r = await Service.UpdateAsync(id, upsert, CancellationToken.None);
        return (r.Ok, r.Error);
    }

    private void OnSlugChanged(string slug) => Input.Slug = slug;
    private void OnSlugValidityChanged(bool valid) => SlugIsValid = valid;
    private void OnTagsChanged(List<string> tags) => Input.Tags = tags;
    private void OnSeoChanged(SeoMetadata seo) => Input.Seo = seo;
    private void OnCoverUploaded(UploadedFile? file) { if (file is not null) Input.CoverImagePath = file.Path; }
    private void ClearCover() => Input.CoverImagePath = "";

    public sealed record class BlogPostInput
    {
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public Guid? AuthorId { get; set; }
        public string CoverImagePath { get; set; } = "";
        public int ReadTimeMinutes { get; set; }
        public List<string> Tags { get; set; } = [];
        public string Intro { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Draft;
        public SeoMetadata Seo { get; set; } = new();

        public static BlogPostInput From(BlogPost p) => new()
        {
            Title = p.Title,
            Slug = p.Slug,
            Description = p.Description,
            Category = p.Category,
            AuthorId = p.AuthorId,
            CoverImagePath = p.CoverImagePath,
            ReadTimeMinutes = p.ReadTimeMinutes,
            Tags = [.. p.Tags],
            Intro = p.Intro,
            Status = p.Status,
            Seo = p.Seo
        };

        public BlogPostUpsert ToUpsert(ArticleBody body) => new(
            Slug, Title, Description, Category, AuthorId, CoverImagePath,
            ReadTimeMinutes, [.. Tags], Intro, Status, Seo, body);
    }
}
