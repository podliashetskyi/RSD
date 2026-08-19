using System.Text;
using Microsoft.Extensions.Options;

namespace RSD.Web.Services.Seo;

public sealed class RobotsTxtProvider(IOptions<SeoOptions> Options) : IRobotsTxtProvider
{
    // Retrieval and user-action bots put pages inside AI answers — always welcome.
    // Googlebot is deliberately never named: it must never risk being blocked.
    private static readonly string[] RetrievalBots =
    [
        "OAI-SearchBot", "ChatGPT-User", "Claude-SearchBot", "Claude-User",
        "PerplexityBot", "Perplexity-User", "Applebot", "Amazonbot",
    ];

    // Training crawlers are a separate, deliberate policy decision (off by default).
    private static readonly string[] TrainingBots =
    [
        "GPTBot", "ClaudeBot", "anthropic-ai", "CCBot", "Google-Extended", "Applebot-Extended",
    ];

    public string Build(string baseUrl)
    {
        var opts = Options.Value.Robots;
        if (!string.IsNullOrWhiteSpace(opts.CustomBody)) return opts.CustomBody;
        if (opts.DisallowAll) return "User-agent: *\nDisallow: /\n";
        return DefaultBody(baseUrl.TrimEnd('/'), opts.BlockAiTraining);
    }

    private static string DefaultBody(string root, bool blockAiTraining)
    {
        var sb = new StringBuilder();
        AppendGroup(sb, RetrievalBots, "Disallow: /admin/", "Disallow: /preview/", "Allow: /");
        if (blockAiTraining) AppendGroup(sb, TrainingBots, "Disallow: /");
        AppendGroup(sb, ["*"], "Disallow: /admin/", "Disallow: /preview/");
        sb.Append("Sitemap: ").Append(root).Append("/sitemap.xml\n");
        return sb.ToString();
    }

    private static void AppendGroup(StringBuilder sb, IReadOnlyList<string> agents, params string[] rules)
    {
        foreach (var agent in agents) sb.Append("User-agent: ").Append(agent).Append('\n');
        foreach (var rule in rules) sb.Append(rule).Append('\n');
        sb.Append('\n');
    }
}
