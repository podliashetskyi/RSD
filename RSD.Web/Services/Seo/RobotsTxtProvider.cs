using Microsoft.Extensions.Options;

namespace RSD.Web.Services.Seo;

public sealed class RobotsTxtProvider(IOptions<SeoOptions> Options) : IRobotsTxtProvider
{
    public string Build(string baseUrl)
    {
        var opts = Options.Value.Robots;
        if (!string.IsNullOrWhiteSpace(opts.CustomBody)) return opts.CustomBody;
        var root = baseUrl.TrimEnd('/');
        if (opts.DisallowAll)
        {
            return "User-agent: *\nDisallow: /\n";
        }
        return $"User-agent: *\nDisallow: /admin/\nDisallow: /preview/\nSitemap: {root}/sitemap.xml\n";
    }
}
