using System;
using System.Threading;
using System.Threading.Tasks;
using Fundo.Applications.Apllication.UseCases.Authentication.Commands;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases.Authentication.Commands
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly Mock<ICommandSqlDb<Users>> _userRepositoryMock;
        private readonly RegisterUserCommandHandler _handler;

        public RegisterUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<ICommandSqlDb<Users>>();
            _handler = new RegisterUserCommandHandler(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var request = new RegisterUserCommand(
                new RegisterUserRequestDto(
                    "test@example.com",
                    "Test123!",
                    "Test",
                    "User",
                    2
                )
            );

            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Users>()))
                .ReturnsAsync((It.IsAny<Users>()));

            _userRepositoryMock.Setup(r => r.SaveAsyncChanges(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("User registered successfully", result.Message);
            Assert.True(result.Data);

            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Users>()), Times.Once);
            _userRepositoryMock.Verify(r => r.SaveAsyncChanges(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_RepositoryFailure_ShouldThrowException()
        {
            // Arrange
            var request = new RegisterUserCommand(
                new RegisterUserRequestDto(
                    "test@example.com",
                    "Test123!",
                    "Test",
                    "User",
                    2
                )
            );

            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Users>()))
                .ReturnsAsync((It.IsAny<Users>()));

            _userRepositoryMock.Setup(r => r.SaveAsyncChanges(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
            
            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Users>()), Times.Once);
            _userRepositoryMock.Verify(r => r.SaveAsyncChanges(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateUserWithCorrectData()
        {
            // Arrange
            var email = "test@example.com";
            var name = "Test";
            var lastName = "User";
            var roleId = 2;

            var request = new RegisterUserCommand(
                new RegisterUserRequestDto(
                    email,
                    "Test123!",
                    name,
                    lastName,
                    roleId
                )
            );

            Users capturedUser = null;

            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Users>()))
                .Callback<Users>(u => capturedUser = u)
                .ReturnsAsync((It.IsAny<Users>()));

            _userRepositoryMock.Setup(r => r.SaveAsyncChanges(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedUser);
            Assert.Equal(email, capturedUser.Email);
            Assert.Equal(name, capturedUser.FirstName);
            Assert.Equal(lastName, capturedUser.LastName);
            Assert.Equal(roleId, capturedUser.RoleId);
            // La contraseña debe estar hasheada, no podemos verificar su valor directamente
            Assert.NotEqual("Test123!", capturedUser.Password);
        }
    }
}
