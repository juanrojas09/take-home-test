using System;
using System.Reflection;
using System.Text.Json.Serialization;

using Fundo.Applications.Infrastructure.Bootstrap;
using Fundo.Applications.WebApi.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Fundo.Applications.Application.Bootstrap;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Fundo.Applications.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
    
            services.AddEndpointsApiExplorer();
            services.AddApplicationServices();
            
          
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            
            services.AddTransient<AuthenticationMiddleware>();
          
            services.AddInfrastructure(Configuration);
            services.AddSwaggerServices();

            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                            Configuration.GetSection("JWT").GetValue<string>("Key"))),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
          
            Console.WriteLine($">>> Environment: {env.EnvironmentName}");
            
            // Habilitar el manejador global de excepciones al inicio del pipeline
            app.UseExceptionHandler();
            
            app.UseCors(builder =>
            {
                builder.WithOrigins(
                   "https://localhost:4200",
                        "http://localhost:4200"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI();
      
            app.UseMiddleware<AuthenticationMiddleware>();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
