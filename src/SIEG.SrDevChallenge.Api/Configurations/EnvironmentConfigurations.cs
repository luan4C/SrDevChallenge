using System;
using SIEG.SrDevChallenge.Infrastructure.Messaging.Models;
using SIEG.SrDevChallenge.Api.Middlewares;

namespace SIEG.SrDevChallenge.Api.Configurations;

public static class EnvironmentConfigurations
{
    public static WebApplicationBuilder ConfigureEnvironment(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<RabbitMqConfigurations>().Bind(builder.Configuration.GetSection("RabbitMQ"))
        .ValidateDataAnnotations().ValidateOnStart();

        builder.Services.AddOptions<ApiKeySettings>().Bind(builder.Configuration.GetSection("ApiKeySettings"))
        .ValidateDataAnnotations().ValidateOnStart();
        
        return builder;    
    }
}
