using SIEG.SrDevChallenge.Api.Middlewares;

namespace SIEG.SrDevChallenge.Api.Configurations;

public static class EndpointsConfigurations
{
    public static IServiceCollection ConfigureMiddlewareServices(this IServiceCollection services)
    {
        services.AddTransient<GlobalExceptionHandlerMiddleware>();
        services.AddScoped<ApiKeyMiddleware>();
        
        return services;
    }
}
