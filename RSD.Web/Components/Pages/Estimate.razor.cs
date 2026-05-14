#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Sections.Contact;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Estimates;

namespace RSD.Web.Components.Pages;

public partial class Estimate(IProjectEstimateService EstimateService) : ComponentBase
{
    private enum WizardStep { Step1Platform, Step2Complexity, Step3Timeline, Step4Submit, Success }

    private WizardStep CurrentStep { get; set; } = WizardStep.Step1Platform;
    private WizardState State { get; set; } = new();
    private EstimatorContactModel ContactModel { get; set; } = new();
    private FormStatus Status { get; set; } = FormStatus.Idle;
    private string ErrorText { get; set; } = "";

    private bool IsBusy => Status == FormStatus.Submitting;

    private int StepNumber => CurrentStep switch
    {
        WizardStep.Step1Platform => 1,
        WizardStep.Step2Complexity => 2,
        WizardStep.Step3Timeline => 3,
        WizardStep.Step4Submit => 4,
        _ => 4,
    };

    private string HeroSubtitle => CurrentStep switch
    {
        WizardStep.Step1Platform => "Quickly calculate the estimated budget for your project by selecting your project type and desired complexity below.",
        WizardStep.Step2Complexity => "Select the complexity level to get a more accurate budget estimate for your project.",
        WizardStep.Step3Timeline => "Choose your implementation timeline to refine the preliminary estimate.",
        WizardStep.Step4Submit => "Review your preliminary estimate and share contact details so we can follow up.",
        _ => "",
    };

    private bool CanGoNext => CurrentStep switch
    {
        WizardStep.Step1Platform => State.Platform is not null && State.Domain is not null,
        WizardStep.Step2Complexity => State.Complexity is not null,
        WizardStep.Step3Timeline => State.Timeline is not null,
        _ => false,
    };

    private (decimal Min, decimal Max) EstimateRange =>
        State is { Platform: { } p, Domain: { } d, Complexity: { } c, Timeline: { } t }
            ? EstimatePricing.Compute(p, d, c, t)
            : (0m, 0m);

    private static string FormatMoney(decimal value) => value.ToString("N0", CultureInfo.InvariantCulture);

    private static string FromPrice(ProjectPlatform platform) =>
        $"from $ {EstimatePricing.BasePrice(platform).ToString("N0", CultureInfo.InvariantCulture)}";

    private void SetPlatform(ProjectPlatform p) => State = State with { Platform = p };
    private void SetDomain(ProjectDomain d) => State = State with { Domain = d };
    private void SetComplexity(ProjectComplexity c) => State = State with { Complexity = c };
    private void SetTimeline(ProjectTimeline t) => State = State with { Timeline = t };

    private void GoNext()
    {
        if (!CanGoNext) return;
        CurrentStep = CurrentStep switch
        {
            WizardStep.Step1Platform => WizardStep.Step2Complexity,
            WizardStep.Step2Complexity => WizardStep.Step3Timeline,
            WizardStep.Step3Timeline => WizardStep.Step4Submit,
            _ => CurrentStep,
        };
    }

    private void GoBack()
    {
        CurrentStep = CurrentStep switch
        {
            WizardStep.Step2Complexity => WizardStep.Step1Platform,
            WizardStep.Step3Timeline => WizardStep.Step2Complexity,
            WizardStep.Step4Submit => WizardStep.Step3Timeline,
            _ => CurrentStep,
        };
        Status = FormStatus.Idle;
        ErrorText = "";
    }

    private async Task SubmitAsync()
    {
        if (!string.IsNullOrWhiteSpace(ContactModel.Hp))
        {
            Status = FormStatus.Success;
            CurrentStep = WizardStep.Success;
            return;
        }
        if (State.Platform is null || State.Domain is null || State.Complexity is null || State.Timeline is null)
        {
            Status = FormStatus.Error;
            ErrorText = "Please complete every step before submitting.";
            return;
        }

        Status = FormStatus.Submitting;
        ErrorText = "";

        var input = new ProjectEstimateInput(
            State.Platform.Value, State.Domain.Value, State.Complexity.Value, State.Timeline.Value,
            ContactModel.Name, ContactModel.Email, ContactModel.Company, ContactModel.Description);

        var result = await EstimateService.SubmitAsync(input, CancellationToken.None);

        if (result.Ok)
        {
            Status = FormStatus.Success;
            CurrentStep = WizardStep.Success;
            return;
        }
        Status = FormStatus.Error;
        ErrorText = string.IsNullOrWhiteSpace(result.Error) ? "Something went wrong. Please try again." : result.Error;
    }

    public sealed record class WizardState
    {
        public ProjectPlatform? Platform { get; init; }
        public ProjectDomain? Domain { get; init; }
        public ProjectComplexity? Complexity { get; init; }
        public ProjectTimeline? Timeline { get; init; }
    }

    public sealed class EstimatorContactModel
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [StringLength(200)]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email.")]
        [StringLength(320)]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please enter your company.")]
        [StringLength(200)]
        public string Company { get; set; } = "";

        [Required(ErrorMessage = "Please describe your project.")]
        [StringLength(8000, MinimumLength = 1)]
        public string Description { get; set; } = "";

        public string Hp { get; set; } = "";
    }
}
