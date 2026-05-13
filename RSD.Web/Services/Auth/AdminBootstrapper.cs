using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;

namespace RSD.Web.Services.Auth;

public sealed class AdminBootstrapper(
    IServiceScopeFactory ScopeFactory,
    IConfiguration Config,
    ILogger<AdminBootstrapper> Log) : IHostedService
{
    private const string EmailEnvVar = "RSD_BOOTSTRAP_ADMIN_EMAIL";
    private const string PasswordEnvVar = "RSD_BOOTSTRAP_ADMIN_PASSWORD";

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = ScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (await db.Users.AnyAsync(ct))
        {
            Log.LogInformation("Admin bootstrapper: users already exist; skipping.");
            return;
        }

        var (email, password) = ReadCredentials();
        if (!HaveCredentials(email, password))
        {
            Log.LogWarning("Admin bootstrapper: {Email} or {Password} env var is empty; first admin not created.", EmailEnvVar, PasswordEnvVar);
            return;
        }

        await SeedAsync(scope.ServiceProvider, email, password);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private (string Email, string Password) ReadCredentials() =>
        (Config[EmailEnvVar] ?? "", Config[PasswordEnvVar] ?? "");

    private static bool HaveCredentials(string email, string password) =>
        !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);

    private async Task SeedAsync(IServiceProvider services, string email, string password)
    {
        var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = services.GetRequiredService<UserManager<AdminUser>>();

        await EnsureAdminRoleAsync(roleMgr);
        var user = await CreateAdminUserAsync(userMgr, email, password);
        if (user is null) return;
        await AssignAdminRoleAsync(userMgr, user);
        Log.LogInformation("Admin bootstrapper: first admin '{Email}' created.", email);
    }

    private async Task EnsureAdminRoleAsync(RoleManager<IdentityRole> roleMgr)
    {
        if (await roleMgr.RoleExistsAsync(AdminRoles.Admin)) return;
        var result = await roleMgr.CreateAsync(new IdentityRole(AdminRoles.Admin));
        if (!result.Succeeded) Log.LogError("Failed to create Admin role: {Errors}", FormatErrors(result.Errors));
    }

    private async Task<AdminUser?> CreateAdminUserAsync(UserManager<AdminUser> userMgr, string email, string password)
    {
        var user = new AdminUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = email,
        };
        var result = await userMgr.CreateAsync(user, password);
        if (result.Succeeded) return user;
        Log.LogError("Failed to create admin user: {Errors}", FormatErrors(result.Errors));
        return null;
    }

    private async Task AssignAdminRoleAsync(UserManager<AdminUser> userMgr, AdminUser user)
    {
        var result = await userMgr.AddToRoleAsync(user, AdminRoles.Admin);
        if (!result.Succeeded) Log.LogError("Failed to add admin to Admin role: {Errors}", FormatErrors(result.Errors));
    }

    private static string FormatErrors(IEnumerable<IdentityError> errors) =>
        string.Join("; ", errors.Select(e => $"{e.Code}: {e.Description}"));
}
