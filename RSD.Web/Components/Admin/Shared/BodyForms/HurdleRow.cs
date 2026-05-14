using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Shared.BodyForms;

public sealed record class HurdleRow
{
    public string Heading { get; set; } = "";
    public string Body { get; set; } = "";

    public static HurdleRow From(ChallengeHurdle h) => new() { Heading = h.Heading, Body = h.Body };
    public ChallengeHurdle ToEntity() => new(Heading, Body);
}
