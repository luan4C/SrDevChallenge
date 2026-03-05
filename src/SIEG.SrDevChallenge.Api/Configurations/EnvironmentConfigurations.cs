using System;
using SIEG.SrDevChallenge.Infrastructure.Messaging.Models;
using SIEG.SrDevChallenge.Api.Middlewares;

namespace SIEG.SrDevChallenge.Api.Configurations;

public static class EnvironmentConfigurations
{
    public static IServiceCollection ConfigureEnvironment(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApiKeySettings>().Bind(configuration.GetSection("ApiKeySettings"))
        .ValidateDataAnnotations().ValidateOnStart();
        
        return services;    
    }
}
