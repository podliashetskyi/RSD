#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Team;

public partial class TeamEdit(ITeamMemberService Service, NavigationManager Nav)
{
    [Parameter] public Guid? Id { get; set; }
    private TeamInput Input { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool IsCreate => Id is null;

    private void OnAvatarUploaded(UploadedFile? file)
    {
        if (file is not null) Input.AvatarPath = file.Path;
    }

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/team"); return; }
        Input = TeamInput.From(existing);
    }

    private async Task SaveAsync()
    {
        try
        {
            var entity = Input.ToEntity(Id);
            var (ok, error) = await PersistAsync(entity);
            if (!ok) { ErrorMessage = error; return; }
            Nav.NavigateTo("/admin/team");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Save failed: {ex.Message}";
        }
    }

    private async Task<(bool Ok, string Error)> PersistAsync(TeamMember entity)
    {
        if (IsCreate)
        {
            var created = await Service.CreateAsync(entity, CancellationToken.None);
            return (created.Ok, created.Error);
        }
        var updated = await Service.UpdateAsync(entity, CancellationToken.None);
        return (updated.Ok, updated.Error);
    }

    public sealed record class TeamInput
    {
        [Required]
        [StringLength(FieldLimits.Team.Name)]
        public string Name { get; set; } = "";
        [StringLength(FieldLimits.Team.Role)]
        public string Role { get; set; } = "";
        [StringLength(FieldLimits.Team.AvatarPath)]
        public string AvatarPath { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        public bool IsManagement { get; set; }
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";
        [StringLength(FieldLimits.Team.SocialUrl)]
        public string LinkedInUrl { get; set; } = "";
        [StringLength(FieldLimits.Team.SocialUrl)]
        public string XUrl { get; set; } = "";
        [StringLength(FieldLimits.Team.SocialUrl)]
        public string GitHubUrl { get; set; } = "";
        [StringLength(FieldLimits.Team.Email)]
        public string Email { get; set; } = "";

        public static TeamInput From(TeamMember m) => new()
        {
            Name = m.Name,
            Role = m.Role,
            AvatarPath = m.AvatarPath,
            Status = m.Status,
            DisplayOrder = m.DisplayOrder,
            IsManagement = m.IsManagement,
            Slug = m.Slug,
            LinkedInUrl = m.LinkedInUrl,
            XUrl = m.XUrl,
            GitHubUrl = m.GitHubUrl,
            Email = m.Email,
        };

        public TeamMember ToEntity(Guid? id) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Slug = Slug,
            Name = Name,
            Role = Role,
            AvatarPath = AvatarPath,
            Status = Status,
            DisplayOrder = DisplayOrder,
            IsManagement = IsManagement,
            LinkedInUrl = LinkedInUrl,
            XUrl = XUrl,
            GitHubUrl = GitHubUrl,
            Email = Email,
        };
    }
}
