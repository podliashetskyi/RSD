#pragma warning disable S1144, S4487, S2933
using System.Collections.Generic;

namespace RSD.Web.Components.Sections.Contact;

public partial class ContactSection
{
    private static readonly IReadOnlyList<ContactPoint> Points =
    [
        new("Phone",   ["+1 (415) 555-1234"],                                   IsLink: false),
        new("Email",   ["hello@nexatech.io"],                                   IsLink: false),
        new("Address", ["San Francisco, CA 94102", "Business Center, Suite 100"], IsLink: true),
    ];

    private static readonly IReadOnlyList<MessengerLink> Messengers =
    [
        new("WhatsApp", "images/contact/messenger/icon-whatsapp-large.svg", "images/contact/messenger/icon-whatsapp-small.svg", "#06d94c"),
        new("Telegram", "images/contact/messenger/icon-telegram-large.svg", "images/contact/messenger/icon-telegram-small.svg", "#269cd9"),
        new("Viber",    "images/contact/messenger/icon-viber-large.svg",    "images/contact/messenger/icon-viber-small.svg",    "#7d519e"),
    ];

    private static readonly IReadOnlyList<SocialLink> Socials =
    [
        new("LinkedIn", "images/contact/social/icon-linkedin.svg", "#"),
        new("Twitter",  "images/contact/social/icon-twitter.svg",  "#"),
        new("Reddit",   "images/contact/social/icon-reddit.svg",   "#"),
        new("Facebook", "images/contact/social/icon-facebook.svg", "#"),
    ];
}

public record ContactPoint(string Label, IReadOnlyList<string> Lines, bool IsLink);
public record MessengerLink(string Label, string LargeIconSrc, string SmallIconSrc, string BgColor);
public record SocialLink(string Label, string IconSrc, string Href);
