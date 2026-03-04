using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SIEG.SrDevChallenge.Application.IoC;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(opt =>
        {
            opt.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        return services;
    }   
}
