using RSD.Web.Services.Mcp.Tools;

namespace RSD.Web.Services.Mcp;

public static class McpServiceCollectionExtensions
{
    public static IServiceCollection AddRsdMcp(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<McpOptions>(configuration.GetSection(McpOptions.SectionName));
        if (!configuration.GetValue($"{McpOptions.SectionName}:Enabled", false)) return services;

        services.AddMcpServer()
                .WithHttpTransport()
                .WithTools<ContentTools>();
        return services;
    }
}
