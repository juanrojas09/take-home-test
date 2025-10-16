// filepath: c:\Users\VICTUS\OneDrive\Escritorio\take-home-test\backend\src\Fundo.Services.Tests\Integration\Fundo.Applications.WebApi\Controllers\AuthControllerTests.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Fundo.Applications.WebApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;
using System.Linq;
using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Apllication.UseCases.Authentication.Commands;
using Fundo.Applications.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Services.Tests.Integration.Fundo.Applications.WebApi.Controllers
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly ApplicationDbContext _dbContext;
        private const string TEST_EMAIL = "admin@fundo.com";
        private const string TEST_PASSWORD = "Admin123!"; 

        public AuthControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
             
                });
            });
            
        
            var scope = _factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

    
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOk()
        {
         
            var loginData = new
            {
                Email = TEST_EMAIL,
                Password = TEST_PASSWORD
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            Assert.True(responseObject.GetProperty("isSuccess").GetBoolean());
            Assert.True(responseObject.TryGetProperty("data", out JsonElement data));
            Assert.True(data.TryGetProperty("token", out _));
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange - Usamos credenciales inválidas pero con formato adecuado
            var loginData = new
            {
                Email = "invalid_test@test.com",
                Password = "InvalidPass123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            // Assert
            Assert.NotEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// note that this test has to rollback the changes in the database after running
        /// </summary>
        [Fact]
        public async Task Register_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var testEmail = $"test_{Guid.NewGuid()}@test.com";
            var registerData = new RegisterUserRequestDto(testEmail, "TestPass123!", "Test", "TestLastName", 2);
         
            try
            {
                // Act
                var response = await _client.PostAsJsonAsync("/api/auth/register", registerData);
    
                // Assert
                Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                Assert.True(responseObject.GetProperty("isSuccess").GetBoolean());
            }
            finally
            {
 
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == testEmail);
                if (user != null)
                {
                    _dbContext.Users.Remove(user);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        [Fact]
        public async Task Register_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange - Datos inválidos para el registro
            var registerData = new
            {
                FirstName = "",
                LastName = "User",
                Email = "invalid-email",
                Password = "short" 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerData);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var duplicateEmail = $"duplicate_{Guid.NewGuid()}@test.com";
            
            // Primer registro
            var registerData1 = new
            {
                FirstName = "Test",
                LastName = "User",
                Email = duplicateEmail,
                Password = "TestPass123!"
            };

            // Registro duplicado
            var registerData2 = new
            {
                FirstName = "Another",
                LastName = "User",
                Email = duplicateEmail, // Mismo email
                Password = "Different123!"
            };

            // Act
            await _client.PostAsJsonAsync("/api/auth/register", registerData1);
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerData2);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
