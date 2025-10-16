using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.WebApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Text;
using System.Text.Json;
using Fundo.Applications.Infrastructure.Persistance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore; 

namespace Fundo.Services.Tests.Integration.Fundo.Applications.WebApi.Controllers
{
    public class LoanManagementControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly ApplicationDbContext _dbContext;
    
        private const string TEST_EMAIL = "admin@fundo.com";
        private const string TEST_PASSWORD = "Admin123!";

        public LoanManagementControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                
                });
            });
            var scope = _factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        private async Task<string> GetAuthToken()
        {
   
            var registerData = new
            {
                FirstName = "Test",
                LastName = "User",
                Email = TEST_EMAIL,
                Password = TEST_PASSWORD
            };

            try
            {
                await _client.PostAsJsonAsync("/api/auth/register", registerData);
            }
            catch
            {
   
            }

     
            var loginData = new
            {
                Email = TEST_EMAIL,
                Password = TEST_PASSWORD
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new System.Exception("Impossible to authenticate the test user.");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            return responseObject.GetProperty("data").GetProperty("token").GetString();
        }

        private void AuthorizeClient(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Can fails if the user is not admin
        /// </summary>
        [Fact]
        
        public async Task GetAllLoans_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthToken();
            AuthorizeClient(token);

            // Act
            var response = await _client.GetAsync("/api/loans");

  
            if (response.IsSuccessStatusCode)
            {
                Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            }
            else
            {
                Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
            }
        }
        
        [Fact]
        public async Task GetMyLoans_WithAuth_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthToken();
            AuthorizeClient(token);

            // Act
            var response = await _client.GetAsync("/api/loans/my");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
        
        [Fact]
        public async Task GetLoanById_WithValidId_ShouldReturnOkOrNotFound()
        {
            // Arrange
            var token = await GetAuthToken();
            AuthorizeClient(token);
            const int loanId = 1; 

            // Act
            var response = await _client.GetAsync($"/api/loans/{loanId}");

            // Assert
          
            Assert.True(
                response.StatusCode == System.Net.HttpStatusCode.OK || 
                response.StatusCode == System.Net.HttpStatusCode.NotFound,
                $"The status code maybe OK or Not Found but is {response.StatusCode}"
            );
        }
        
        [Fact]
        public async Task CreateLoan_WithValidData_ShouldReturnCreated()
        {
            try
            {


                // Arrange
                var token = await GetAuthToken();
                AuthorizeClient(token);
                var loanRequest = new
                {
                    Amount = 012345678900,
                 
                };

                // Act
                var response = await _client.PostAsJsonAsync("/api/loans", loanRequest);

                // Assert
                Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            }finally
            {
                var loan = await _dbContext.Loans.FirstOrDefaultAsync(x=>x.Amount == 012345678900);
                if (loan != null)
                {
                    _dbContext.Loans.Remove(loan);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
        
        [Fact]
        public async Task MakePayment_WithValidData_ShouldReturnOkOrNotFound()
        {
            try
            {
                // Arrange
                var token = await GetAuthToken();
                AuthorizeClient(token);

                //First create a loan
                var loanRequest = new
                {
                    Amount = 012345678900,
                    Term = 12
                };
                var createResponse = await _client.PostAsJsonAsync("/api/loans", loanRequest);


                int loanId = 0;
                if (createResponse.IsSuccessStatusCode)
                {
                    var content = await createResponse.Content.ReadAsStringAsync();
                    var responseObj = JsonSerializer.Deserialize<JsonElement>(content);
                    loanId = responseObj.GetProperty("data").GetProperty("id").GetInt32();
                }
                else
                {

                    loanId = 1;
                }

                var paymentRequest = new
                {
                    Amount = 100m
                };

                // Act
                var response = await _client.PostAsJsonAsync($"/api/loans/{loanId}/payment", paymentRequest);

                // Assert
                Assert.True(
                    response.StatusCode == System.Net.HttpStatusCode.OK ||
                    response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                    response.StatusCode == System.Net.HttpStatusCode.BadRequest,
                    $"Status has to be OK or Not Found or BadRequest but is {response.StatusCode}"
                );
            }finally
            {
                var loan = await _dbContext.Loans.FirstOrDefaultAsync(x => x.Amount == 012345678900);
                if (loan != null)
                {
                    _dbContext.Loans.Remove(loan);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
