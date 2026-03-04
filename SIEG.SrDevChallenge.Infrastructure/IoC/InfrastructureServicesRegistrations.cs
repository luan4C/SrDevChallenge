using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIEG.SrDevChallenge.Application.Contracts;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Contexts;
using SIEG.SrDevChallenge.Infrastructure.Persistence.Repositories;
using SIEG.SrDevChallenge.Infrastructure.Services;

namespace SIEG.SrDevChallenge.Infrastructure.IoC;

public static class InfrastructureServicesRegistrations
{
    public static IServiceCollection ConfigurePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SrDevChallengeContext>(opt => opt.UseMongoDB(configuration.GetConnectionString("Mongo")));

        services.AddScoped<IDocumentoFiscalRepository, DocumentoFiscalRepository>();
        return services;
    }
    public static IServiceCollection ConfigureXMLServices(this IServiceCollection services)
    {
        services.AddScoped<IDocumentSchemaValidator, DocumentoFiscalXMLSchemaValidator>();

        return services;
    }
}
