using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RSD.Web.Data;
using RSD.Web.Services.Auth;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration;

[Collection(nameof(PostgresCollection))]
public sealed class AdminBootstrapperTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task OnEmptyDb_WithValidEnvVars_CreatesFirstAdmin()
    {
        await using var sp = BuildIdentityProvider(Postgres.ConnectionString);
        await EnsureUsersTableEmptyAsync(sp);

        var config = ConfigWith("AdminBootstrapTest@example.com", "BootstrapPass1!");
        var bootstrapper = new AdminBootstrapper(sp.GetRequiredService<IServiceScopeFactory>(), config, NullLogger<AdminBootstrapper>.Instance);
        await bootstrapper.StartAsync(CancellationToken.None);

        using var scope = sp.CreateScope();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();
        var user = await userMgr.FindByEmailAsync("AdminBootstrapTest@example.com");
        user.Should().NotBeNull();
        (await userMgr.IsInRoleAsync(user!, AdminRoles.Admin)).Should().BeTrue();
        user!.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task OnNonEmptyDb_NoOps()
    {
        await using var sp = BuildIdentityProvider(Postgres.ConnectionString);
        await EnsureUsersTableEmptyAsync(sp);
        await SeedExistingUserAsync(sp);

        var config = ConfigWith("would-have-been-created@example.com", "WouldHaveBeen1!");
        var bootstrapper = new AdminBootstrapper(sp.GetRequiredService<IServiceScopeFactory>(), config, NullLogger<AdminBootstrapper>.Instance);
        await bootstrapper.StartAsync(CancellationToken.None);

        using var scope = sp.CreateScope();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();
        var would = await userMgr.FindByEmailAsync("would-have-been-created@example.com");
        would.Should().BeNull();
    }

    private static async Task EnsureUsersTableEmptyAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        db.Users.RemoveRange(db.Users);
        db.UserRoles.RemoveRange(db.UserRoles);
        db.Roles.RemoveRange(db.Roles);
        await db.SaveChangesAsync();
    }

    private static async Task SeedExistingUserAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();
        var user = new AdminUser
        {
            UserName = "preexisting@example.com",
            Email = "preexisting@example.com",
            EmailConfirmed = true,
            DisplayName = "Pre-Existing",
        };
        var result = await userMgr.CreateAsync(user, "PreExisting1!");
        result.Succeeded.Should().BeTrue();
    }

    private static IConfiguration ConfigWith(string email, string password) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RSD_BOOTSTRAP_ADMIN_EMAIL"] = email,
                ["RSD_BOOTSTRAP_ADMIN_PASSWORD"] = password,
            })
            .Build();

    private static ServiceProvider BuildIdentityProvider(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddIdentity<AdminUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        return services.BuildServiceProvider();
    }
}
