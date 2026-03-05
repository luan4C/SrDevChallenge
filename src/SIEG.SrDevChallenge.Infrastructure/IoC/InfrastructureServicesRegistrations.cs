using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Infrastructure.Messaging;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Contexts;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Mongo;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Repositories;
using SIEG.SrDevChallenge.Infrastructure.Services;

namespace SIEG.SrDevChallenge.Infrastructure.IoC;

public static class InfrastructureServicesRegistrations
{
    public static IServiceCollection ConfigurePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = new MongoClient(configuration.GetConnectionString("Mongo"));
            return client.GetDatabase("appdb"); // Replace with your actual database name
        });
        services.AddDbContext<SrDevChallengeContext>(opt => opt.UseMongoDB(configuration.GetConnectionString("Mongo")));
        services.AddScoped<MongoIndexInitializer>();

        services.AddScoped<IDocumentoFiscalRepository, DocumentoFiscalRepository>();
        services.AddScoped<IDocumentoFiscaisResumoMensalRepository, DocumentoFiscaisResumoMensalRepository>();
        return services;
    }
    public static IServiceCollection ConfigureXMLServices(this IServiceCollection services)
    {
        services.AddScoped<IDocumentSchemaValidator, DocumentoFiscalXMLSchemaValidator>();

        return services;
    }

    public static IServiceCollection ConfigureRabbitMQ(this IServiceCollection services)
    {
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddHostedService<RabbitMqEventConsumer>();

        return services;
    }
}
