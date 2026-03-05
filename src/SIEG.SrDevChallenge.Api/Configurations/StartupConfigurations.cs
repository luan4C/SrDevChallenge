using System;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Mongo;
using SIEG.SrDevChallenge.Api.Middlewares;

namespace SIEG.SrDevChallenge.Api.Configurations;

public static class StartupConfigurations
{
    public static WebApplication ConfigureMiddlewares(this WebApplication app)
    {
        // Middleware de tratamento global de exceções deve ser o primeiro
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        
        // Middleware de autenticação por API Key
        app.UseMiddleware<ApiKeyMiddleware>();
        
        return app;
    }

    public static WebApplication ConfigureMongoStartup(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var mongoInitializer = scope.ServiceProvider.GetRequiredService<MongoIndexInitializer>();
        mongoInitializer.InitializeAsync().GetAwaiter().GetResult();
        return app;
    }
}
