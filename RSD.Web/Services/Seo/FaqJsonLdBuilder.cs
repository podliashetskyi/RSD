using System.Text.Json;
using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Seo;

/// <summary>
/// FAQPage schema mirroring the visibly rendered Q&amp;A pairs (parity rule).
/// System.Text.Json-serialized only (script-safe escaping).
/// </summary>
internal static class FaqJsonLdBuilder
{
    internal static string Build(IReadOnlyList<FaqItem> items) =>
        items.Count == 0 ? "" : JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "FAQPage",
            ["mainEntity"] = items.Select(QuestionNode).ToList(),
        });

    private static Dictionary<string, object> QuestionNode(FaqItem item) => new()
    {
        ["@type"] = "Question",
        ["name"] = item.Question,
        ["acceptedAnswer"] = new Dictionary<string, object>
        {
            ["@type"] = "Answer",
            ["text"] = item.AnswerHtml,
        },
    };
}
