using FluentAssertions;
using Microsoft.Extensions.Options;
using RSD.Web.Services.Seo;

namespace RSD.Web.Tests.Unit.Seo;

/// <summary>
/// Per-bot robots policy: AI retrieval/user bots are explicitly welcomed (they put pages
/// inside AI answers), Googlebot is never named or blocked, admin/preview stay closed,
/// and blocking AI training crawlers is a deliberate config decision, off by default.
/// </summary>
public sealed class RobotsTxtProviderTests
{
    private const string Root = "https://remsoft.dev";

    private static readonly string[] RetrievalBots =
    [
        "OAI-SearchBot", "ChatGPT-User", "Claude-SearchBot", "Claude-User",
        "PerplexityBot", "Perplexity-User", "Applebot", "Amazonbot",
    ];

    private static readonly string[] TrainingBots =
    [
        "GPTBot", "ClaudeBot", "anthropic-ai", "CCBot", "Google-Extended", "Applebot-Extended",
    ];

    [Fact]
    public void CustomBody_StillWinsVerbatim()
    {
        Build(new RobotsOptions { CustomBody = "User-agent: *\nDisallow: /secret/" })
            .Should().Be("User-agent: *\nDisallow: /secret/");
    }

    [Fact]
    public void DisallowAll_StillBlocksEverything_ForStaging()
    {
        var body = Build(new RobotsOptions { DisallowAll = true });

        body.Should().Contain("User-agent: *").And.Contain("Disallow: /");
        foreach (var bot in RetrievalBots) body.Should().NotContain(bot);
    }

    [Fact]
    public void Default_AllowsEveryRetrievalAndUserBot_ButKeepsAdminClosedForThem()
    {
        var body = Build(new RobotsOptions());

        foreach (var bot in RetrievalBots) body.Should().Contain($"User-agent: {bot}");
        // A named group overrides *, so the retrieval group must carry its own disallows.
        var retrievalGroup = body[..body.IndexOf("User-agent: *", StringComparison.Ordinal)];
        retrievalGroup.Should().Contain("Allow: /")
            .And.Contain("Disallow: /admin/")
            .And.Contain("Disallow: /preview/");
    }

    [Fact]
    public void Default_KeepsAdminAndPreviewClosed_AndAdvertisesSitemap()
    {
        var body = Build(new RobotsOptions());

        body.Should().Contain("User-agent: *")
            .And.Contain("Disallow: /admin/")
            .And.Contain("Disallow: /preview/")
            .And.Contain($"Sitemap: {Root}/sitemap.xml");
    }

    [Fact]
    public void Default_DoesNotNameTrainingBots()
    {
        var body = Build(new RobotsOptions());

        foreach (var bot in TrainingBots) body.Should().NotContain($"User-agent: {bot}");
    }

    [Fact]
    public void BlockAiTraining_DisallowsEachTrainingBot()
    {
        var body = Build(new RobotsOptions { BlockAiTraining = true });

        foreach (var bot in TrainingBots) body.Should().Contain($"User-agent: {bot}");
        TrainingSection(body).Should().Contain("Disallow: /");
    }

    [Fact]
    public void GooglebotIsNeverNamed_SoItCanNeverBeBlocked()
    {
        foreach (var opts in new[] { new RobotsOptions(), new RobotsOptions { BlockAiTraining = true } })
        {
            var body = Build(opts);
            body.Should().NotContain("User-agent: Googlebot");
        }
    }

    private static string Build(RobotsOptions robots) =>
        new RobotsTxtProvider(Microsoft.Extensions.Options.Options.Create(new SeoOptions { Robots = robots })).Build(Root);

    private static string TrainingSection(string body) =>
        body[body.IndexOf("User-agent: GPTBot", StringComparison.Ordinal)..];
}
