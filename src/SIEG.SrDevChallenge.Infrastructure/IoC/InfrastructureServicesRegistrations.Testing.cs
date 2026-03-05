using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIEG.SrDevChallenge.Application.Contracts;

namespace SIEG.SrDevChallenge.Infrastructure.IoC;

public static class InfrastructureServicesRegistrationsExtensions
{
    public static bool IsTestingEnvironment(this IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return env == "Testing";
    }
}