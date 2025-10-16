using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Application.UseCases.LoansOperations.Queries;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases.LoansOperations.Queries
{
    public class GetLoanByIdQueryHandlerTests
    {
        private readonly Mock<IQuerySqlDb<Loans>> _loanQuerySqlDbMock;
        private readonly Mock<IContextService> _contextServiceMock;
        private readonly Mock<ILogger<GetLoanByIdQueryHandler>> _loggerMock;
        private readonly GetLoanByIdQueryHandler _handler;
        private readonly Mock<IValidator<int>> _validator;

        public GetLoanByIdQueryHandlerTests()
        {
            _loanQuerySqlDbMock = new Mock<IQuerySqlDb<Loans>>();
            _contextServiceMock = new Mock<IContextService>();
            _loggerMock = new Mock<ILogger<GetLoanByIdQueryHandler>>();
            _validator= new Mock<IValidator<int>>();
            
            _handler = new GetLoanByIdQueryHandler(
                _loggerMock.Object,
                _contextServiceMock.Object,
                _loanQuerySqlDbMock.Object,
                _validator.Object
            );
        }

        [Fact]
        public async Task Handle_ValidId_ShouldReturnLoan()
        {
            // Arrange
            const int loanId = 1;
            var userId = 5;
            
            var applicant = Users.CreateNew("test@example.com", "hashedPassword", "Test", "User", 2);
            var loan = Loans.CreateNew(1000, userId);
            _validator.Setup(x=>x.ValidateAsync(loanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
            typeof(Loans).GetProperty("Id")!.SetValue(loan, loanId);
            typeof(Loans).GetProperty("Applicant")!.SetValue(loan, applicant);
            
            _contextServiceMock.Setup(c => c.GetUserIdByToken()).Returns(userId);
            
            _loanQuerySqlDbMock
                .Setup(q => q.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Loans, bool>>>(), 
                    false, 
                    It.IsAny<Expression<Func<Loans, object>>>(),
                    It.IsAny<Expression<Func<Loans, object>>>()))
                .ReturnsAsync(loan);
            
            _loanQuerySqlDbMock
                .Setup(q => q.AnyAsync(It.IsAny<Expression<Func<Loans, bool>>>()))
                .ReturnsAsync(true);

            var query = new GetLoanByIdQuery(loanId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Loan retrieved successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(loanId, result.Data.Id);
            Assert.Equal(1000, result.Data.Amount);
            Assert.Equal(userId, result.Data.ApplicantId);
        }

        [Fact]
        public async Task Handle_LoanNotFound_ShouldReturnFailure()
        {
            // Arrange
            const int loanId = 999; 
            var userId = 5;
            
            _contextServiceMock.Setup(c => c.GetUserIdByToken()).Returns(userId);
            _validator.Setup(x=>x.ValidateAsync(loanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
            _loanQuerySqlDbMock
                .Setup(q => q.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Loans, bool>>>(), 
                    false, 
                    It.IsAny<Expression<Func<Loans, object>>>(),
                    It.IsAny<Expression<Func<Loans, object>>>()))
                .ReturnsAsync((Loans)null);
            
            _loanQuerySqlDbMock
                .Setup(q => q.AnyAsync(It.IsAny<Expression<Func<Loans, bool>>>()))
                .ReturnsAsync(false);

            var query = new GetLoanByIdQuery(loanId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Loan not found", result.Message);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("Loan not found", result.Errors);
        }

        [Fact]
        public async Task Handle_ValidationFailed_ShouldReturnFailure()
        {
            // Arrange
            const int loanId = 1;
            _validator.Setup(x=>x.ValidateAsync(loanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
                {
                    new FluentValidation.Results.ValidationFailure("Id", "Invalid loan ID")
                }));
            _contextServiceMock.Setup(c => c.GetUserIdByToken()).Returns((int?)null);
            
            var query = new GetLoanByIdQuery(loanId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Errores de validacion", result.Message);
            Assert.NotEmpty(result.Errors);
        }

        
    }
}
