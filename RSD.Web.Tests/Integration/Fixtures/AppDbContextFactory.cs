using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RSD.Web.Data;
using RSD.Web.Data.Interceptors;

namespace RSD.Web.Tests.Integration.Fixtures;

/// <summary>
/// Builds a clean AppDbContext + service provider for a single test, applying
/// migrations once per fixture lifetime. Each test gets its own DbContext
/// instance; the test is responsible for cleaning up rows it created.
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
        services.AddDbContext<AppDbContext>((sp, options) =>
            options.UseNpgsql(connectionString)
                   .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));
        return services.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync()
    {
        if (Services is not null) await Services.DisposeAsync();
    }
}
