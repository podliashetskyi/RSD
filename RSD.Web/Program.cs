using Microsoft.EntityFrameworkCore;
using RSD.Web.Components;
using RSD.Web.Data;
using RSD.Web.Data.Interceptors;
using RSD.Web.Services.Audit;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Email;
using RSD.Web.Services.Imaging;
using RSD.Web.Services.Preview;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
builder.Services.AddScoped<IAuditLog, AuditLog>();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
           .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

builder.Services.AddHostedService<MigrationHostedService>();

builder.Services
    .AddRsdStorage()
    .AddRsdImaging(builder.Configuration)
    .AddRsdSlugs()
    .AddRsdCache(builder.Configuration)
    .AddRsdEmail(builder.Configuration, builder.Environment)
    .AddRsdPreview(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAntiforgery();
app.UseOutputCache();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
