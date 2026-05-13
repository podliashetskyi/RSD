using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Data.Interceptors;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Content;
using RSD.Web.Services.Email;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Tests.Integration.Fixtures;

/// <summary>
/// Per-test service provider with AppDbContext + audit interceptor + Slugger +
/// a no-op IPublicPageCache + the 9 simple content services. Migrates once,
/// reuses the schema for subsequent tests.
/// </summary>
public sealed class AppDbContextFactory(string connectionString) : IAsyncDisposable
{
    private bool MigrationsApplied;
    private ServiceProvider? Services;

    public async Task<AppDbContext> CreateAsync()
    {
        Services ??= BuildServiceProvider(connectionString);
        var ctx = Services.GetRequiredService<AppDbContext>();
        if (!MigrationsApplied)
        {
            await ctx.Database.MigrateAsync();
            MigrationsApplied = true;
        }
        return ctx;
    }

    public IServiceProvider Provider => Services ??= BuildServiceProvider(connectionString);

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContextFactory<AppDbContext>((sp, options) =>
            options.UseNpgsql(connectionString)
                   .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));
        services.AddScoped<AppDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
        services.AddScoped<ISlugger, Slugger>();
        services.AddSingleton<IPublicPageCache, NoopPublicPageCache>();
        services.AddSingleton<CapturingEmailSender>();
        services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<CapturingEmailSender>());
        services.Configure<EmailOptions>(o => { o.From = "test@local"; o.ContactTo = "inbox@local"; });
        services.AddRsdContent();
        return services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync()
    {
        if (Services is not null) await Services.DisposeAsync();
    }

    private sealed class NoopPublicPageCache : IPublicPageCache
    {
        public Task EvictForAsync<TEntity>(Guid id, CancellationToken ct) where TEntity : ContentEntity => Task.CompletedTask;
        public Task EvictListAsync<TEntity>(CancellationToken ct) where TEntity : ContentEntity => Task.CompletedTask;
        public Task EvictAllAsync(CancellationToken ct) => Task.CompletedTask;
    }
}

public sealed class CapturingEmailSender : IEmailSender
{
    public List<EmailMessage> Sent { get; } = [];
    public Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        Sent.Add(message);
        return Task.CompletedTask;
    }
}
