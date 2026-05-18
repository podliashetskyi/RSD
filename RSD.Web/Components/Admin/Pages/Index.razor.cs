#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Pages;

public partial class Index(IDbContextFactory<AppDbContext> DbFactory) : ComponentBase
{
    private IReadOnlyList<DashboardMetric> Metrics { get; set; } = [];
    private IReadOnlyList<DashboardMetric> Drafts { get; set; } = [];
    private IReadOnlyList<AuditLogEntry> RecentAudits { get; set; } = [];
    private IReadOnlyList<UploadedFile> RecentUploads { get; set; } = [];
    private int TotalDrafts => Drafts.Sum(d => d.Value);

    private static IReadOnlyList<QuickLink> QuickLinks { get; } =
    [
        new("New blog post", "/admin/blog/new"),
        new("New case study", "/admin/cases/new"),
        new("New service", "/admin/services/new"),
        new("New product", "/admin/products/new"),
        new("Media library", "/admin/media"),
    ];

    protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        var openInbox = await db.ContactSubmissions.AsNoTracking().CountAsync(x => !x.IsHandled);
        var openEstimates = await db.ProjectEstimates.AsNoTracking().CountAsync(x => !x.IsHandled);
        var uploadCount = await db.UploadedFiles.AsNoTracking().CountAsync();
        Drafts = await LoadDraftsAsync(db);
        Metrics = BuildMetrics(openInbox, openEstimates, TotalDrafts, uploadCount);
        RecentAudits = await LoadRecentAuditsAsync(db);
        RecentUploads = await LoadRecentUploadsAsync(db);
    }

    private static IReadOnlyList<DashboardMetric> BuildMetrics(int openInbox, int openEstimates, int totalDrafts, int uploadCount) =>
    [
        new("Open inbox", openInbox, "/admin/inbox", "Review submissions"),
        new("Open estimates", openEstimates, "/admin/estimates", "Review estimates"),
        new("Draft content", totalDrafts, "/admin/blog", "Open content"),
        new("Media files", uploadCount, "/admin/media", "Open media"),
    ];

    private static async Task<IReadOnlyList<DashboardMetric>> LoadDraftsAsync(AppDbContext db) =>
    [
        new("Blog posts", await CountDraftsAsync(db.BlogPosts)),
        new("Case studies", await CountDraftsAsync(db.Cases), "/admin/cases"),
        new("Products", await CountDraftsAsync(db.Products), "/admin/products"),
        new("Services", await CountDraftsAsync(db.Services), "/admin/services"),
    ];

    private static Task<int> CountDraftsAsync<T>(IQueryable<T> query) where T : ContentEntity =>
        query.AsNoTracking().CountAsync(x => x.Status == ContentStatus.Draft && !x.IsDeleted);

    private static Task<List<AuditLogEntry>> LoadRecentAuditsAsync(AppDbContext db) =>
        db.AuditLogEntries.AsNoTracking().OrderByDescending(x => x.At).Take(5).ToListAsync();

    private static Task<List<UploadedFile>> LoadRecentUploadsAsync(AppDbContext db) =>
        db.UploadedFiles.AsNoTracking().OrderByDescending(x => x.UploadedAt).Take(5).ToListAsync();

    private static string FormatLocalTime(DateTime value) => value.ToLocalTime().ToString("MMM d, HH:mm");

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:0.#} KB";
        return $"{bytes / (1024.0 * 1024.0):0.##} MB";
    }

    private sealed record DashboardMetric(string Label, int Value, string Href = "/admin/blog", string ActionLabel = "Open");
    private sealed record QuickLink(string Label, string Href);
}
