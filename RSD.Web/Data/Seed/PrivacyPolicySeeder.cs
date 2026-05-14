using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class PrivacyPolicySeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<PrivacyPolicy>(Db, Slugger)
{
    protected override Task<IReadOnlyList<PrivacyPolicy>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<PrivacyPolicy> items =
        [
            new()
            {
                Slug = "privacy-policy",
                Status = ContentStatus.Published,
                PublishedAt = DateTime.UtcNow,
                Title = "Privacy Policy",
                LastUpdatedAt = new DateOnly(2026, 2, 13),
                BodyHtml = BodyHtml,
            },
        ];
        return Task.FromResult(items);
    }

    private const string BodyHtml = """
        <p>This chapter provides an overview of the basic principles of design, such as balance, contrast, and hierarchy.</p>
        <h2>1. Data We Collect</h2>
        <p>We collect information that you voluntarily provide when using our estimator tool:</p>
        <ul>
          <li>Contact Information: Name, email address, and phone number (provided in Step 4).</li>
          <li>Project Details: Project type, complexity level, and implementation timeline selected during your estimation process.</li>
          <li>Usage Data: Information on how you interact with our calculator.</li>
        </ul>
        <h2>2. Use of Data</h2>
        <p>We use the collected data to:</p>
        <ul>
          <li>Provide you with a detailed and accurate project cost estimate.</li>
          <li>Contact you to discuss your project requirements after you submit the form.</li>
          <li>Improve the functionality and user experience of our estimator tool.</li>
        </ul>
        <h2>3. Data Storage and Security</h2>
        <p>Your data is securely stored and encrypted. We implement industry-standard security measures to prevent unauthorized access, disclosure, or alteration of your personal information.</p>
        <h2>4. Data Sharing and Disclosure</h2>
        <p>We do not sell or rent your personal data to third parties. We may only share information with trusted service providers who assist us in operating our website or contacting our business, subject to confidentiality agreements.</p>
        <h2>5. Your Rights</h2>
        <p>You have the right to:</p>
        <ul>
          <li>Access the personal data we hold about you.</li>
          <li>Request the correction or deletion of your personal information.</li>
          <li>Withdraw your consent for data processing at any time.</li>
        </ul>
        <h2>6. Contact Us</h2>
        <p>If you have any questions about this Privacy Policy, please contact us at:</p>
        <ul>
          <li>Email: <a href="mailto:hello@remsoft.dev">hello@remsoft.dev</a></li>
          <li>Address: San Francisco, CA 94102</li>
          <li>Phone: +1 (415) 555-1234</li>
        </ul>
        """;
}
