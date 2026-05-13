using FluentAssertions;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Email.EmailTemplates;

namespace RSD.Web.Tests.Unit.Email;

public sealed class EmailTemplatesRenderTests
{
    [Fact]
    public void ForgotPassword_RendersSubjectAndBody()
    {
        var msg = ForgotPasswordTemplate.Render("user@example.com", "Mark", "https://example.com/reset?token=abc");

        msg.To.Should().Be("user@example.com");
        msg.Subject.Should().Be("Reset your RSD Admin password");
        msg.HtmlBody.Should().Contain("Reset your password");
        msg.TextBody.Should().Contain("https://example.com/reset?token=abc");
    }

    [Fact]
    public void ForgotPassword_EncodesUserContent()
    {
        var msg = ForgotPasswordTemplate.Render("user@example.com", "<script>alert(1)</script>", "https://x.test/?q=1&z=2");
        msg.HtmlBody.Should().NotContain("<script>");
        msg.HtmlBody.Should().Contain("&lt;script&gt;");
    }

    [Fact]
    public void UserInvite_RendersSubjectAndBody()
    {
        var msg = UserInviteTemplate.Render("invitee@example.com", "Mark", "https://example.com/invite?token=xyz");

        msg.To.Should().Be("invitee@example.com");
        msg.Subject.Should().Contain("invited");
        msg.HtmlBody.Should().Contain("Accept the invite");
        msg.TextBody.Should().Contain("https://example.com/invite?token=xyz");
    }

    [Fact]
    public void ContactSubmission_RendersAllSubmissionFields()
    {
        var submission = new ContactSubmission
        {
            Name = "Alice",
            Email = "alice@example.com",
            Subject = "Hello",
            Message = "I am interested in working with you.",
        };

        var msg = ContactSubmissionTemplate.Render("inbox@example.com", submission, "https://example.com/admin/inbox");

        msg.To.Should().Be("inbox@example.com");
        msg.Subject.Should().Contain("Hello");
        msg.HtmlBody.Should().Contain("Alice");
        msg.HtmlBody.Should().Contain("I am interested");
        msg.TextBody.Should().Contain("https://example.com/admin/inbox");
    }

    [Fact]
    public void ContactSubmission_EncodesUserContent()
    {
        var submission = new ContactSubmission
        {
            Name = "<b>Bob</b>",
            Email = "bob@example.com",
            Subject = "<img src=x onerror=alert(1)>",
            Message = "<script>evil()</script>",
        };

        var msg = ContactSubmissionTemplate.Render("inbox@example.com", submission, "https://example.com/admin/inbox");

        msg.HtmlBody.Should().NotContain("<script>");
        msg.HtmlBody.Should().NotContain("<img src=x");
        msg.HtmlBody.Should().Contain("&lt;script&gt;");
    }
}
