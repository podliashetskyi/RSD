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
        services.AddHostedService<SeedRunner>();
        return services;
    }
}
