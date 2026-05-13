using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RSD.Web.Components;
using RSD.Web.Data;
using RSD.Web.Data.Interceptors;
using RSD.Web.Data.Seed;
using RSD.Web.Services.Audit;
using RSD.Web.Services.Auth;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Content;
using RSD.Web.Services.Email;
using RSD.Web.Services.Imaging;
using RSD.Web.Services.Preview;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
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
    .AddRsdSeed();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseOutputCache();

app.MapStaticAssets();
app.MapPost("/admin/logout", async (HttpContext http, SignInManager<AdminUser> signIn) =>
{
    await signIn.SignOutAsync();
    return Results.LocalRedirect("/admin/login");
}).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
