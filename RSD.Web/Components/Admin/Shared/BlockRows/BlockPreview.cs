using System.Text.RegularExpressions;

namespace RSD.Web.Components.Admin.Shared.BlockRows;

internal static partial class BlockPreview
{
    private const int MaxLen = 80;

    public static string Trim(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "—";
        var t = s.Trim().Replace("\n", " ").Replace("\r", " ");
        return t.Length <= MaxLen ? t : t[..MaxLen] + "…";
    }

    public static string StripTags(string html) => TagPattern().Replace(html ?? "", " ");

    [GeneratedRegex(@"<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex TagPattern();
}
