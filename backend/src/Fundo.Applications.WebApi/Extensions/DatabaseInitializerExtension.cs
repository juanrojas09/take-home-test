using System;
using Fundo.Applications.Infrastructure.Persistance;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fundo.Applications.WebApi.Extensions
{
    public static class DatabaseInitializerExtension
    {
        public static IServiceProvider InitializeDatabase(this IServiceProvider serviceProvider)
        {
            try
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();
                logger.LogInformation("Initializing the database...");
                DbInitializer.InitializeAsync(serviceProvider).Wait();
                logger.LogInformation("Database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while initializing the database: {ex.Message}");
                throw;
            }

            return serviceProvider;
        }
    }
}
