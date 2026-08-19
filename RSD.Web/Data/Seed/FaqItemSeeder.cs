using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class FaqItemSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<FaqItem>(Db, Slugger)
{
    protected override Task<IReadOnlyList<FaqItem>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<FaqItem> items =
        [
            Build("How does remote collaboration work with RSD?",
                "<p>We work in your time zone overlap with daily written updates, a shared task board, and weekly demo calls. You always see what is being built and why.</p>",
                "Process", 1),
            Build("How do we start a project?",
                "<p>We begin with a short discovery call, then a written proposal with scope, timeline, and price. Work starts only after you approve it.</p>",
                "Process", 2),
            Build("Who owns the code and intellectual property?",
                "<p>You do. All code, designs, and documentation produced for your project are transferred to you in full under the contract.</p>",
                "Contract", 3),
            Build("What technologies does RSD specialize in?",
                "<p>Our core stack is .NET, Blazor, and cloud platforms, complemented by modern web frontends and mobile development.</p>",
                "Technology", 4),
        ];
        return Task.FromResult(items);
    }

    private static FaqItem Build(string question, string answerHtml, string category, int order) => new()
    {
        Slug = question,
        Status = ContentStatus.Published,
        PublishedAt = DateTime.UtcNow,
        Question = question,
        AnswerHtml = answerHtml,
        Category = category,
        DisplayOrder = order,
    };
}
