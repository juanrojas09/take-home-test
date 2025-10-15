using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Domain.Interfaces;
using Fundo.Applications.Infrastructure.Persistance;
using Fundo.Applications.Infrastructure.Persistance.Repositories;
using Fundo.Applications.Infrastructure.Services.Context;
using Fundo.Applications.Infrastructure.Services.MongoDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fundo.Applications.Infrastructure.Bootstrap;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString=configuration.GetSection("ConnectionStrings").GetValue<string>("DefaultConnection");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });
        services.AddSingleton<IMongoServices, MongoServices>();
        services.AddScoped(typeof(ICommandSqlDb<>), typeof(CommandSqlDb<>));
        services.AddScoped(typeof(IQuerySqlDb<>), typeof(QuerySqlDb<>));
        services.AddScoped<IContextService, ContextService>();
        return services;
    }
}