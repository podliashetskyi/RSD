using System.Globalization;
using System.Web;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Estimates;

namespace RSD.Web.Services.Email.EmailTemplates;

public static class ProjectEstimateTemplate
{
    public static EmailMessage Render(string to, ProjectEstimate estimate, string adminUrl)
    {
        var name = HttpUtility.HtmlEncode(estimate.ContactName);
        var email = HttpUtility.HtmlEncode(estimate.ContactEmail);
        var company = HttpUtility.HtmlEncode(estimate.Company);
        var description = HttpUtility.HtmlEncode(estimate.ProjectDescription);
        var url = HttpUtility.HtmlEncode(adminUrl);
        var summary = HttpUtility.HtmlEncode(EstimatorCatalog.SummaryChip(estimate.Platform, estimate.Domain, estimate.Complexity, estimate.Timeline));
        var range = FormatRange(estimate.EstimateMin, estimate.EstimateMax);

        var html = $"""
            <p>A new project-cost estimate was submitted on the RSD website.</p>
            <p><strong>From:</strong> {name} &lt;{email}&gt;<br/>
            <strong>Company:</strong> {company}<br/>
            <strong>Submitted:</strong> {estimate.SubmittedAt:u}</p>
            <p><strong>Selections:</strong> {summary}<br/>
            <strong>Preliminary range:</strong> {range}</p>
            <hr/>
            <p>{description}</p>
            <hr/>
            <p><a href="{url}">Open admin estimates</a></p>
            """;

        var text = $"""
            New project-cost estimate on RSD website.

            From: {estimate.ContactName} <{estimate.ContactEmail}>
            Company: {estimate.Company}
            Submitted: {estimate.SubmittedAt:u}

            Selections: {EstimatorCatalog.SummaryChip(estimate.Platform, estimate.Domain, estimate.Complexity, estimate.Timeline)}
            Preliminary range: {range}

            {estimate.ProjectDescription}

            Admin: {adminUrl}
            """;

        return new EmailMessage(to, $"[RSD] New estimate: {estimate.Company} — {range}", html, text);
    }

    private static string FormatRange(decimal min, decimal max)
    {
        var culture = CultureInfo.InvariantCulture;
        return $"$ {min.ToString("N0", culture)} – $ {max.ToString("N0", culture)}";
    }
}
