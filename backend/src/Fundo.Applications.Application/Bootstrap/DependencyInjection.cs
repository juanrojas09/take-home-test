using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Fundo.Applications.Application.Bootstrap;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
   
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        
        return services;
    }
}