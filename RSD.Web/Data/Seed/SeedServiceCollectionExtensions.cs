namespace RSD.Web.Data.Seed;

public static class SeedServiceCollectionExtensions
{
    public static IServiceCollection AddRsdSeed(this IServiceCollection services)
    {
        services.AddScoped<ISeeder, TestimonialSeeder>();
        services.AddScoped<ISeeder, TeamMemberSeeder>();
        services.AddScoped<ISeeder, PartnerSeeder>();
        services.AddScoped<ISeeder, ValueSeeder>();
        services.AddScoped<ISeeder, MissionStatSeeder>();
        services.AddScoped<ISeeder, TechStackItemSeeder>();
        services.AddScoped<ISeeder, ContactPointSeeder>();
        services.AddScoped<ISeeder, MessengerLinkSeeder>();
        services.AddScoped<ISeeder, SocialLinkSeeder>();
        services.AddScoped<ISeeder, BlogPostSeeder>();
        services.AddScoped<ISeeder, CaseSeeder>();
        services.AddScoped<ISeeder, ProductSeeder>();
        services.AddScoped<ISeeder, ServiceSeeder>();
        services.AddScoped<ISeeder, TermsOfServiceSeeder>();
        services.AddScoped<ISeeder, PrivacyPolicySeeder>();
        services.AddScoped<ISeeder, PublicPlaceholderCleanupSeeder>();
        services.AddHostedService<SeedRunner>();
        return services;
    }
}
