namespace RSD.Web.Services.Content;

public static class ContentServiceCollectionExtensions
{
    public static IServiceCollection AddRsdContent(this IServiceCollection services)
    {
        services.AddScoped<ITestimonialService, TestimonialService>();
        services.AddScoped<ITeamMemberService, TeamMemberService>();
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<IValueService, ValueService>();
        services.AddScoped<IMissionStatService, MissionStatService>();
        services.AddScoped<ITechStackItemService, TechStackItemService>();
        services.AddScoped<IContactPointService, ContactPointService>();
        services.AddScoped<IMessengerLinkService, MessengerLinkService>();
        services.AddScoped<ISocialLinkService, SocialLinkService>();
        services.AddScoped<IContactSubmissionService, ContactSubmissionService>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<ICaseService, CaseService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IServiceService, ServiceService>();
        return services;
    }
}
