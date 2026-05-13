#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Contact;

public partial class ContactForm(IHttpClientFactory HttpClientFactory, NavigationManager Navigation) : ComponentBase
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
        Status = FormStatus.Submitting;
        ErrorText = "";
        var payload = new
        {
            name = Model.Name,
            email = Model.Email,
            subject = Model.Subject,
            message = Model.Message,
            hp = Model.Hp
        };

        try
        {
            using var client = HttpClientFactory.CreateClient();
            client.BaseAddress = new Uri(Navigation.BaseUri);
            var response = await client.PostAsJsonAsync("api/contact", payload);
            await ApplyResponseAsync(response);
        }
        catch (HttpRequestException)
        {
            Status = FormStatus.Error;
            ErrorText = "We couldn't reach the server. Please try again.";
        }
    }

    private async Task ApplyResponseAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            Status = FormStatus.Success;
            Model = new ContactFormModel();
            return;
        }
        ErrorText = await ReadErrorAsync(response);
        Status = FormStatus.Error;
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        if ((int)response.StatusCode == 429)
        {
            return "Too many submissions. Please wait a few minutes and try again.";
        }
        try
        {
            var body = await response.Content.ReadFromJsonAsync<ApiError>();
            return string.IsNullOrWhiteSpace(body?.Error) ? "Something went wrong. Please try again." : body!.Error;
        }
        catch
        {
            return "Something went wrong. Please try again.";
        }
    }

    private sealed record ApiError(string Error);
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
