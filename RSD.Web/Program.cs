using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using RSD.Web.Components;
using RSD.Web.Data;
using RSD.Web.Data.Interceptors;
using RSD.Web.Data.Seed;
using RSD.Web.Endpoints;
using RSD.Web.Services.Audit;
using RSD.Web.Services.Auth;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Content;
using RSD.Web.Services.Email;
using RSD.Web.Services.Estimates;
using RSD.Web.Services.Imaging;
using RSD.Web.Services.Preview;
using RSD.Web.Services.Seo;
using RSD.Web.Services.Slugs;
using Microsoft.Extensions.FileProviders;
using RSD.Web.Services.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
builder.Services.AddScoped<IAuditLog, AuditLog>();
builder.Services.AddScoped<RSD.Web.Components.Admin.Shared.IToastService, RSD.Web.Components.Admin.Shared.ToastService>();

builder.Services.AddDbContextFactory<AppDbContext>((sp, options) =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
           .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));
builder.Services.AddScoped<AppDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

builder.Services.AddHostedService<MigrationHostedService>();

builder.Services
    .AddRsdAuth()
    .AddRsdStorage()
    .AddRsdImaging(builder.Configuration)
    .AddRsdSlugs()
    .AddRsdCache(builder.Configuration)
    .AddRsdEmail(builder.Configuration, builder.Environment)
    .AddRsdPreview(builder.Configuration)
    .AddRsdContent()
    .AddRsdEstimates()
    .AddRsdSeo(builder.Configuration)
    .AddRsdSeed();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = static async (context, ct) =>
    {
        context.HttpContext.Response.Headers["Retry-After"] = "300";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Too many submissions. Please try again in a few minutes." }, ct);
    };
    options.AddPolicy(ContactSubmitEndpoint.RateLimitPolicy, context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0
        });
    });
    options.AddPolicy(EstimateSubmitEndpoint.RateLimitPolicy, context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/404", "?statusCode={0}");

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseRateLimiter();
app.UseOutputCache();

app.MapStaticAssets();

// Serve runtime uploads (written under wwwroot/uploads after publish, so not in the
// MapStaticAssets manifest). Mounted as a separate provider scoped to /uploads only.
var uploadsRoot = Path.Combine(app.Environment.WebRootPath, "uploads");
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads"
});

app.MapContactSubmit();
app.MapEstimateSubmit();
app.MapSitemap();
app.MapRobots();
app.MapPost("/admin/logout", async (HttpContext http, SignInManager<AdminUser> signIn) =>
{
    await signIn.SignOutAsync();
    return Results.LocalRedirect("/admin/login");
}).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
