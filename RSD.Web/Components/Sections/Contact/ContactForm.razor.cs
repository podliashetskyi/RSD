#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.Contact;

public partial class ContactForm(IContactSubmissionService Service) : ComponentBase
{
    private ContactFormModel Model { get; set; } = new();
    private FormStatus Status { get; set; } = FormStatus.Idle;
    private string ErrorText { get; set; } = "";
    private bool IsBusy => Status == FormStatus.Submitting;

    private async Task HandleSubmitAsync()
    {
        if (!Model.AcceptsTerms)
        {
            Status = FormStatus.Error;
            ErrorText = "Please accept the Terms of Service to continue.";
            return;
        }
        if (!string.IsNullOrWhiteSpace(Model.Hp))
        {
            Status = FormStatus.Success;
            Model = new ContactFormModel();
            return;
        }

        Status = FormStatus.Submitting;
        ErrorText = "";

        var input = new ContactSubmissionInput(Model.Name, Model.Email, Model.Subject, Model.Message);
        var result = await Service.SubmitAsync(input, CancellationToken.None);

        if (result.Ok)
        {
            Status = FormStatus.Success;
            Model = new ContactFormModel();
            return;
        }
        Status = FormStatus.Error;
        ErrorText = string.IsNullOrWhiteSpace(result.Error) ? "Something went wrong. Please try again." : result.Error;
    }
}

public sealed class ContactFormModel
{
    [Required(ErrorMessage = "Please enter your name.")]
    [StringLength(200)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Please enter your email.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email.")]
    [StringLength(320)]
    public string Email { get; set; } = "";

    [StringLength(500)]
    public string Subject { get; set; } = "";

    [Required(ErrorMessage = "Please write a message.")]
    [StringLength(8000, MinimumLength = 1)]
    public string Message { get; set; } = "";

    public string Hp { get; set; } = "";
    public bool AcceptsTerms { get; set; }
}

public enum FormStatus
{
    Idle,
    Submitting,
    Success,
    Error
}
