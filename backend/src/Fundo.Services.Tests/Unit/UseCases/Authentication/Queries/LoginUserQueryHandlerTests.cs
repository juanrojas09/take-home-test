using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Fundo.Applications.Application.UseCases.Authentication.Queries;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases.Authentication.Queries
{
    public class LoginUserQueryHandlerTests
    {
        private readonly Mock<IQuerySqlDb<Users>> _userRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IConfigurationSection> _jwtSectionMock;
        private readonly LoginUserQueryHandler _handler;

        public LoginUserQueryHandlerTests()
        {
            _userRepositoryMock = new Mock<IQuerySqlDb<Users>>();
            _configurationMock = new Mock<IConfiguration>();
            _jwtSectionMock = new Mock<IConfigurationSection>();
            

            _jwtSectionMock.Setup(s => s["Key"]).Returns("test_secret_key_that_is_long_enough_for_jwt_signature");
            _configurationMock.Setup(c => c.GetSection("JWT")).Returns(_jwtSectionMock.Object);
            
            _handler = new LoginUserQueryHandler(_userRepositoryMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCredentials_ShouldReturnSuccessWithToken()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Test123!";
            var hashedPassword = Users.GeneratePasswordHash(password);
            
            var user = Users.CreateNew(
                email,
                hashedPassword,
                "Test",
                "User",
                2
            );
            
            var request = new LoginUserQuery(new LoginUserDto(email, password));
            
            _userRepositoryMock
                .Setup(r => r.FirstOrDefaultAsNoTrackingAsync(
                    It.IsAny<Expression<Func<Users, bool>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Users, object>>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Successfull login", result.Message);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Token);
        }

        [Fact]
        public async Task Handle_UserNotFound_ShouldReturnNotFoundResponse()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var password = "Test123!";
            
            var request = new LoginUserQuery(new LoginUserDto(email, password));
            
            _userRepositoryMock
                .Setup(r => r.FirstOrDefaultAsNoTrackingAsync(
                    It.IsAny<Expression<Func<Users, bool>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Users, object>>>()))
                .ReturnsAsync((Users)null);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("The user with the provided email does not exist.", result.Message);
            Assert.Null(result.Data);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Handle_InvalidPassword_ShouldReturnForbiddenResponse()
        {
            // Arrange
            var email = "test@example.com";
            var correctPassword = "Test123!";
            var wrongPassword = "WrongPassword123!";
            var hashedPassword = Users.GeneratePasswordHash(correctPassword);
            
            var user = Users.CreateNew(
                email,
                hashedPassword,
                "Test",
                "User",
                2
            );
            
            var request = new LoginUserQuery(new LoginUserDto(email, wrongPassword));
            
            _userRepositoryMock
                .Setup(r => r.FirstOrDefaultAsNoTrackingAsync(
                    It.IsAny<Expression<Func<Users, bool>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Users, object>>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid Credentials.", result.Message);
            Assert.Null(result.Data);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public async Task Handle_RepositoryException_ShouldThrowException()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Test123!";
            
            var request = new LoginUserQuery(new LoginUserDto(email, password));
            
            _userRepositoryMock
                .Setup(r => r.FirstOrDefaultAsNoTrackingAsync(
                    It.IsAny<Expression<Func<Users, bool>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Users, object>>>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
