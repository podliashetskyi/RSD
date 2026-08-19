namespace RSD.Web.Services.Seo;

/// <summary>
/// Composes absolute URLs for SEO output from the canonical origin and stored paths.
/// Entity image paths are stored rootless ("images/...", "uploads/...") — do not couple
/// this to the storage layer's root-relative URLs.
/// </summary>
public static class AbsoluteUrl
{
    public static string Compose(string origin, string path)
    {
        if (path.Length == 0) return "";
        if (path.StartsWith("http://") || path.StartsWith("https://")) return path;
        var root = origin.TrimEnd('/');
        return path[0] == '/' ? $"{root}{path}" : $"{root}/{path}";
    }
}
