#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Components.Admin.Shared.Blocks;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Preview;

namespace RSD.Web.Components.Admin.Pages.Blog;

public partial class BlogEdit(
    IBlogService Service,
    IFilterService Filters,
    ITeamMemberService TeamMembers,
    NavigationManager Nav,
    IToastService Toasts,
    PreviewLink Preview) : ComponentBase
{
    private const string DefaultAuthorAvatarSrc = "images/logo.svg";

    [Parameter] public Guid? Id { get; set; }

    private BlogPostInput Input { get; set; } = new();
    private ArticleBodyForm Body { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool SlugIsValid { get; set; } = true;
    private string LoadedSlug { get; set; } = "";
    private bool IsCreate => Id is null;
    private bool CanSave => SlugIsValid;
    private string PreviewUrl => string.IsNullOrEmpty(LoadedSlug) ? "" : Preview.Build("blog", LoadedSlug);

    private IReadOnlyList<string> CategoryOptions { get; set; } = [];
    private IReadOnlyList<string> TagOptions { get; set; } = [];
    private IReadOnlyList<AuthorOption> AuthorOptions { get; set; } = [];
    private AuthorOption? SelectedAuthor => AuthorOptions.FirstOrDefault(a => a.Id == Input.AuthorId);

    protected override async Task OnInitializedAsync()
    {
        var categories = await Filters.ListByTypeAsync(FilterType.BlogCategory, CancellationToken.None);
        var tags = await Filters.ListByTypeAsync(FilterType.BlogTag, CancellationToken.None);
        var authors = await TeamMembers.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 200), CancellationToken.None);
        CategoryOptions = [.. categories.Select(f => f.Label)];
        TagOptions = [.. tags.Select(f => f.Label)];
        AuthorOptions = [.. authors.OrderBy(a => a.DisplayOrder).ThenBy(a => a.Name).Select(AuthorOption.From)];
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/blog"); return; }
        Input = BlogPostInput.From(existing);
        Body = ArticleBodyForm.From(existing.BodyBlocks);
        LoadedSlug = existing.Slug;
    }

    private async Task SaveAsync()
    {
        if (!CanSave) { ErrorMessage = "Resolve validation errors before saving."; return; }
        try
        {
            var upsert = Input.ToUpsert(Body.ToEntity());
            if (IsCreate) await HandleCreateAsync(upsert);
            else await HandleUpdateAsync(Id!.Value, upsert);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Save failed: {ex.Message}";
        }
    }

    private async Task HandleCreateAsync(BlogPostUpsert upsert)
    {
        var created = await Service.CreateAsync(upsert, CancellationToken.None);
        if (!created.Ok) { ErrorMessage = created.Error; return; }
        Toasts.Show("Post created.", ToastKind.Success);
        Nav.NavigateTo($"/admin/blog/{created.Value}");
    }

    private async Task HandleUpdateAsync(Guid id, BlogPostUpsert upsert)
    {
        var updated = await Service.UpdateAsync(id, upsert, CancellationToken.None);
        if (!updated.Ok) { ErrorMessage = updated.Error; return; }
        Toasts.Show("Post saved.", ToastKind.Success);
        Nav.NavigateTo("/admin/blog");
    }

    private void OnSlugChanged(string slug) => Input.Slug = slug;
    private void OnSlugValidityChanged(bool valid) => SlugIsValid = valid;
    private void OnTagsChanged(List<string> tags) => Input.Tags = tags;
    private void OnSeoChanged(SeoMetadata seo) => Input.Seo = seo;
    private void OnCoverUploaded(UploadedFile? file) { if (file is not null) Input.CoverImagePath = file.Path; }
    private void OnCoverAltChanged(string alt) => Input.CoverImageAlt = alt;
    private void ClearCover() => Input.CoverImagePath = "";

    private static string AvatarSrc(string avatarPath) =>
        string.IsNullOrWhiteSpace(avatarPath) ? DefaultAuthorAvatarSrc : avatarPath;

    private sealed record AuthorOption(Guid Id, string Name, string Role, string AvatarPath)
    {
        public static AuthorOption From(TeamMember member) => new(
            member.Id,
            member.Name,
            member.Role,
            AvatarSrc(member.AvatarPath));
    }

    public sealed record class BlogPostInput
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(FieldLimits.BlogPost.Title)]
        public string Title { get; set; } = "";
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";
        [StringLength(FieldLimits.BlogPost.Summary)]
        public string Summary { get; set; } = "";
        [StringLength(FieldLimits.BlogPost.Description)]
        public string Description { get; set; } = "";
        [StringLength(FieldLimits.BlogPost.Category)]
        public string Category { get; set; } = "";
        public Guid? AuthorId { get; set; }
        [StringLength(FieldLimits.BlogPost.CoverImagePath)]
        public string CoverImagePath { get; set; } = "";
        [StringLength(FieldLimits.BlogPost.CoverImageAlt)]
        public string CoverImageAlt { get; set; } = "";
        public int ReadTimeMinutes { get; set; }
        public List<string> Tags { get; set; } = [];
        [StringLength(FieldLimits.BlogPost.Intro)]
        public string Intro { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Draft;
        public SeoMetadata Seo { get; set; } = new();

        public static BlogPostInput From(BlogPost p) => new()
        {
            Title = p.Title,
            Slug = p.Slug,
            Summary = p.Summary,
            Description = p.Description,
            Category = p.Category,
            AuthorId = p.AuthorId,
            CoverImagePath = p.CoverImagePath,
            CoverImageAlt = p.CoverImageAlt,
            ReadTimeMinutes = p.ReadTimeMinutes,
            Tags = [.. p.Tags],
            Intro = p.Intro,
            Status = p.Status,
            Seo = p.Seo
        };

        public BlogPostUpsert ToUpsert(ArticleBody body) => new(
            Slug, Title, Summary, Description, Category, AuthorId, CoverImagePath, CoverImageAlt,
            ReadTimeMinutes, [.. Tags], Intro, Status, Seo, body);
    }
}
