using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class MessengerLinkSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<MessengerLink>(Db, Slugger)
{
    protected override Task<IReadOnlyList<MessengerLink>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<MessengerLink> items =
        [
            Build("WhatsApp", "images/contact/messenger/icon-whatsapp-large.svg", "images/contact/messenger/icon-whatsapp-small.svg", "#06d94c", 1),
            Build("Telegram", "images/contact/messenger/icon-telegram-large.svg", "images/contact/messenger/icon-telegram-small.svg", "#269cd9", 2),
            Build("Viber",    "images/contact/messenger/icon-viber-large.svg",    "images/contact/messenger/icon-viber-small.svg",    "#7d519e", 3),
        ];
        return Task.FromResult(items);
    }

    private static MessengerLink Build(string label, string largeIcon, string smallIcon, string bgColor, int order) => new()
    {
        Slug = label,
        Status = ContentStatus.Published,
        PublishedAt = DateTime.UtcNow,
        Label = label,
        LargeIconPath = largeIcon,
        SmallIconPath = smallIcon,
        BgColor = bgColor,
        Href = "",
        DisplayOrder = order,
    };
}
