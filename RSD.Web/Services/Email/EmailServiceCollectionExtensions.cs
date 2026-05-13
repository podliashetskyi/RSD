namespace RSD.Web.Services.Email;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddRsdEmail(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        if (ShouldUseLoggingSender(configuration, env))
        {
            services.AddSingleton<IEmailSender, LoggingEmailSender>();
        }
        else
        {
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        }
        return services;
    }

    private static bool ShouldUseLoggingSender(IConfiguration configuration, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) return true;
        var smtpHost = configuration.GetValue("Email:Smtp:Host", "");
        return string.IsNullOrWhiteSpace(smtpHost);
    }
}
