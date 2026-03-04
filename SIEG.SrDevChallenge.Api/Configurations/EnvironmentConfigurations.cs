using System;
using SIEG.SrDevChallenge.Infrastructure.Messaging.Models;

namespace SIEG.SrDevChallenge.Api.Configurations;

public static class EnvironmentConfigurations
{
    public static WebApplicationBuilder ConfigureEnvironment(this WebApplicationBuilder builder)
    {
        DotNetEnv.Env.Load();
        builder.Services.AddOptions<RabbitMqConfigurations>().Bind(builder.Configuration.GetSection("RabbitMQ"))
        .ValidateDataAnnotations().ValidateOnStart();
        
        return builder;    
    }
}
