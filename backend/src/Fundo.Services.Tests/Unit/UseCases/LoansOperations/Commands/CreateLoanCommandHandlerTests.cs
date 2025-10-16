using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Application.UseCases.LoansOperations.Commands;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Enums;
using Fundo.Applications.Domain.Interfaces;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases.LoansOperations.Commands
{
    public class CreateLoanCommandHandlerTests
    {
        private readonly Mock<ICommandSqlDb<Loans>> _loanCommandSqlDbMock;
        private readonly Mock<IContextService> _contextServiceMock;
        private readonly CreateLoanCommandHandler _handler;

        public CreateLoanCommandHandlerTests()
        {
            _loanCommandSqlDbMock = new Mock<ICommandSqlDb<Loans>>();
            _contextServiceMock = new Mock<IContextService>();
            _handler = new CreateLoanCommandHandler(_loanCommandSqlDbMock.Object, _contextServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldReturnSuccessWithLoanDetails()
        {
            // Arrange
            const int userId = 1;
            const string userName = "Test User";
            const decimal amount = 1000;
            
            var request = new CreateLoanCommand(new LoanRequestDto(amount));
            
            _contextServiceMock.Setup(cs => cs.GetUserIdByToken()).Returns(userId);
            _contextServiceMock.Setup(cs => cs.GetUserNameByToken()).Returns(userName);
            
            _loanCommandSqlDbMock.Setup(db => db.AddAsync(It.IsAny<Loans>()))
                .ReturnsAsync((It.IsAny<Loans>()));
            
            _loanCommandSqlDbMock.Setup(db => db.SaveAsyncChanges(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Loan created successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(amount, result.Data.CurrentBalance);
            Assert.Equal(userName, result.Data.Applicant.FullName);
            Assert.Equal(LoanStatusesEnum.ACTIVE.ToString(), result.Data.Status);
            

            _loanCommandSqlDbMock.Verify(db => db.AddAsync(It.IsAny<Loans>()), Times.Once);
            _loanCommandSqlDbMock.Verify(db => db.SaveAsyncChanges(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoUserContext_ShouldReturnFailure()
        {
            // Arrange
            const decimal amount = 1000;
            var request = new CreateLoanCommand(new LoanRequestDto(amount));
            
            _contextServiceMock.Setup(cs => cs.GetUserIdByToken()).Returns((int?)null);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("User is not valid.", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task Handle_InvalidAmount_ShouldReturnValidationError()
        {
            // Arrange
            const int userId = 1;
            const decimal invalidAmount = -100; 
            
            var request = new CreateLoanCommand(new LoanRequestDto(invalidAmount));
            
            _contextServiceMock.Setup(cs => cs.GetUserIdByToken()).Returns(userId);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Validation errors occurred", result.Message);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public async Task Handle_DatabaseError_ShouldThrowException()
        {
            // Arrange
            const int userId = 1;
            const decimal amount = 1000;
            
            var request = new CreateLoanCommand(new LoanRequestDto(amount));
            
            _contextServiceMock.Setup(cs => cs.GetUserIdByToken()).Returns(userId);
            
            _loanCommandSqlDbMock.Setup(db => db.AddAsync(It.IsAny<Loans>()))
                .ReturnsAsync(It.IsAny<Loans>());
            
            _loanCommandSqlDbMock.Setup(db => db.SaveAsyncChanges(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
