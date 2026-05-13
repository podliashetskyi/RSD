using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using RSD.Web.Data;

namespace RSD.Web.Services.Auth;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddRsdAuth(this IServiceCollection services)
    {
        ConfigureIdentity(services);
        ConfigureLockout(services);
        ConfigureCookie(services);
        services.AddAuthorization();
        services.AddScoped<IClaimsTransformation, AdminUserClaimsTransformer>();
        services.AddHostedService<AdminBootstrapper>();
        return services;
    }

    private static void ConfigureIdentity(IServiceCollection services)
    {
        services.AddIdentity<AdminUser, IdentityRole>(options => options.User.RequireUniqueEmail = true)
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    }

    private static void ConfigureLockout(IServiceCollection services)
    {
        services.Configure<IdentityOptions>(options =>
        {
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        });
    }

    private static void ConfigureCookie(IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
            options.LoginPath = "/admin/login";
            options.LogoutPath = "/admin/logout";
            options.AccessDeniedPath = "/admin/login";
        });
    }
}
