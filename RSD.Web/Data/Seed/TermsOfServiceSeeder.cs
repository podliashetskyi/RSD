using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class TermsOfServiceSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<TermsOfService>(Db, Slugger)
{
    protected override Task<IReadOnlyList<TermsOfService>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<TermsOfService> items =
        [
            new()
            {
                Slug = "terms-of-service",
                Status = ContentStatus.Published,
                PublishedAt = DateTime.UtcNow,
                Title = "Terms of Service",
                LastUpdatedAt = new DateOnly(2026, 2, 13),
                BodyHtml = BodyHtml,
            },
        ];
        return Task.FromResult(items);
    }

    private const string BodyHtml = """
        <p>Welcome to RemSoft.Dev. By using our Project Cost Estimator tool and services, you agree to comply with and be bound by the following terms and conditions.</p>
        <h2>1. Acceptance of Terms</h2>
        <p>By accessing this website, you confirm that you have read, understood, and agreed to these Terms of Service. If you do not agree, please refrain from using our website or services.</p>
        <h2>2. Services Provided</h2>
        <p>Our tool provides a preliminary cost estimate based on the data you input (project type, complexity, etc.). Please note:</p>
        <ul>
          <li>This result is an approximate estimation, not a final binding contract.</li>
          <li>Final pricing is subject to a detailed discovery phase and a formal agreement signed by both parties.</li>
        </ul>
        <h2>3. User Responsibilities</h2>
        <p>When using the tool, you agree to:</p>
        <ul>
          <li>Provide accurate and truthful information.</li>
          <li>Use the tool only for its intended purposes (estimating software development costs).</li>
          <li>Not attempt to interfere with the website's security or functionality.</li>
        </ul>
        <h2>4. Intellectual Property</h2>
        <p>All content, design elements, and the estimator logic used in this tool are the property of RemSoft.Dev. You may not reproduce or distribute any part of the service without our prior written consent.</p>
        <h2>5. Limitation of Liability</h2>
        <p>While we strive for maximum accuracy, we are not liable for any decisions made based on the preliminary estimates provided by the tool. We are not responsible for any indirect or consequential losses arising from the use of our website.</p>
        <h2>6. Modifications to Services</h2>
        <p>We reserve the right to modify or discontinue any part of the estimator tool or these Terms of Service at any time without prior notice.</p>
        <h2>7. Governing Law</h2>
        <p>These Terms shall be governed by and construed in accordance with the laws of the jurisdiction in which our company is registered.</p>
        <h2>8. Contact Information</h2>
        <p>For any questions regarding these Terms, please reach out to:</p>
        <ul>
          <li>Email: <a href="mailto:contactus@remsoft.dev">contactus@remsoft.dev</a></li>
          <li>Support: <a href="mailto:support@remsoft.dev">support@remsoft.dev</a></li>
        </ul>
        """;
}
