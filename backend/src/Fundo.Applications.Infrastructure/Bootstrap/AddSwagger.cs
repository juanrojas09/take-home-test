using Microsoft.Extensions.DependencyInjection;

namespace Fundo.Applications.Infrastructure.Bootstrap;

public static class AddSwagger
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHttpContextAccessor();
        return services;
    }
    
}