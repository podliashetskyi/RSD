namespace RSD.Web.Services.Seo;

public sealed record SitemapEntry(string Loc, DateTime LastMod);

public interface ISitemapBuilder
{
    Task<IReadOnlyList<SitemapEntry>> BuildAsync(string baseUrl, CancellationToken ct);
}
