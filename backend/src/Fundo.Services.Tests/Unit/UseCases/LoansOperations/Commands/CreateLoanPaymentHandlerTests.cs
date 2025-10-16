using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Application.UseCases.LoansOperations.Commands;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Enums;
using Fundo.Applications.Domain.Exceptions;
using Fundo.Applications.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentValidation;
using FluentValidation.Results;

namespace Fundo.Services.Tests.Unit.UseCases.LoansOperations.Commands
{
    public class CreateLoanPaymentHandlerTests
    {
        private readonly Mock<IQuerySqlDb<Loans>> _loanQuerySqlDbMock;
        private readonly Mock<ICommandSqlDb<Loans>> _loanCommandSqlDbMock;
        private readonly Mock<IContextService> _contextServiceMock;
        private readonly Mock<ILogger<CreateLoanPaymentHandler>> _loggerMock;
        private readonly Mock<IValidator<LoanPaymentRequestDto>> _validatorMock;
        private readonly CreateLoanPaymentHandler _handler;

        public CreateLoanPaymentHandlerTests()
        {
            _loanQuerySqlDbMock = new Mock<IQuerySqlDb<Loans>>();
            _loanCommandSqlDbMock = new Mock<ICommandSqlDb<Loans>>();
            _contextServiceMock = new Mock<IContextService>();
            _loggerMock = new Mock<ILogger<CreateLoanPaymentHandler>>();
            _validatorMock = new Mock<IValidator<LoanPaymentRequestDto>>();

            _handler = new CreateLoanPaymentHandler(
                _loanQuerySqlDbMock.Object,
                _loanCommandSqlDbMock.Object,
                _contextServiceMock.Object,
                _loggerMock.Object,
                _validatorMock.Object
            );
        }

        [Fact]
        public async Task Handle_ValidPayment_ShouldReturnSuccessWithUpdatedLoan()
        {
            // Arrange
            const int loanId = 1;
            const decimal initialBalance = 1000;
            const decimal paymentAmount = 200;
            const decimal expectedRemainingBalance = initialBalance - paymentAmount;
            
            var applicant = Users.CreateNew("test@example.com", "hashedPassword", "Test", "User", 2);
            var loan = Loans.CreateNew(initialBalance, 1);
            
            typeof(Loans).GetProperty("Id")!.SetValue(loan, loanId);
            typeof(Loans).GetProperty("CurrentBalance")!.SetValue(loan, initialBalance);
            typeof(Loans).GetProperty("Applicant")!.SetValue(loan, applicant);
            
            var request = new CreateLoanPaymentCommand(new LoanPaymentRequestDto(loanId, paymentAmount));
            
            
            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<LoanPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            
            _loanQuerySqlDbMock
                .Setup(q => q.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Loans, bool>>>(),
                    true,
                    It.IsAny<Expression<Func<Loans, object>>>()))
                .ReturnsAsync(loan);
            
            _loanCommandSqlDbMock
                .Setup(c => c.UpdateAsync(It.IsAny<Loans>(), false))
                .Verifiable();
            
            _loanCommandSqlDbMock
                .Setup(c => c.SaveAsyncChanges(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            _contextServiceMock
                .Setup(cs => cs.GetUserIdByToken())
                .Returns(1);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Pago de prestamo registrado con exito", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedRemainingBalance, result.Data.CurrentBalance);
            Assert.Equal(LoanStatusesEnum.ACTIVE.ToString(), result.Data.Status);
            
            _loanCommandSqlDbMock.Verify(c => c.UpdateAsync(It.IsAny<Loans>(), false), Times.Once);
            _loanCommandSqlDbMock.Verify(c => c.SaveAsyncChanges(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_FullPayment_ShouldUpdateLoanStatusToPaid()
        {
            // Arrange
            const int loanId = 1;
            const decimal initialBalance = 1000;
            const decimal paymentAmount = 1000; 
            
            var applicant = Users.CreateNew("test@example.com", "hashedPassword", "Test", "User", 2);
            var loan = Loans.CreateNew(initialBalance, 1);
            
            typeof(Loans).GetProperty("Id")!.SetValue(loan, loanId);
            typeof(Loans).GetProperty("CurrentBalance")!.SetValue(loan, initialBalance);
            typeof(Loans).GetProperty("Applicant")!.SetValue(loan, applicant);
            
            var request = new CreateLoanPaymentCommand(new LoanPaymentRequestDto(loanId, paymentAmount));
            
           
            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<LoanPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            
            _loanQuerySqlDbMock
                .Setup(q => q.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Loans, bool>>>(),
                    true,
                    It.IsAny<Expression<Func<Loans, object>>>()))
                .ReturnsAsync(loan);
            
            _loanCommandSqlDbMock
                .Setup(c => c.UpdateAsync(It.IsAny<Loans>(), false))
                .Verifiable();
            
            _loanCommandSqlDbMock
                .Setup(c => c.SaveAsyncChanges(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            _contextServiceMock
                .Setup(cs => cs.GetUserIdByToken())
                .Returns(1);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Data.CurrentBalance);
            Assert.Equal(LoanStatusesEnum.PAID.ToString(), result.Data.Status);
        }

        [Fact]
        public async Task Handle_InvalidPaymentAmount_ShouldReturnValidationError()
        {
            // Arrange
            const int loanId = 1;
            const decimal paymentAmount = -100; 
            
            var request = new CreateLoanPaymentCommand(new LoanPaymentRequestDto(loanId, paymentAmount));
            
           
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Amount", "El monto debe ser mayor que cero")
            };
            var validationResult = new ValidationResult(validationFailures);
            
            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<LoanPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
            
            _contextServiceMock
                .Setup(cs => cs.GetUserIdByToken())
                .Returns(1);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("El monto debe ser mayor que cero", result.Errors);
        }

        [Fact]
        public async Task Handle_PaymentExceedsBalance_ShouldReturnValidationError()
        {
            // Arrange
            const int loanId = 1;
            const decimal initialBalance = 500;
            const decimal paymentAmount = 600; 
            
            var request = new CreateLoanPaymentCommand(new LoanPaymentRequestDto(loanId, paymentAmount));
            
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Amount", "El monto del pago no puede exceder el saldo actual del préstamo")
            };
            var validationResult = new ValidationResult(validationFailures);
            
            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<LoanPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
                
            _contextServiceMock
                .Setup(cs => cs.GetUserIdByToken())
                .Returns(1);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public async Task Handle_LoanNotFound_ShouldReturnValidationError()
        {
            // Arrange
            const int loanId = 999; 
            const decimal paymentAmount = 100;
            
            var request = new CreateLoanPaymentCommand(new LoanPaymentRequestDto(loanId, paymentAmount));
            
        
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("LoanId", "El préstamo no existe")
            };
            var validationResult = new ValidationResult(validationFailures);
            
            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<LoanPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
                
            _contextServiceMock
                .Setup(cs => cs.GetUserIdByToken())
                .Returns(1);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public async Task Handle_ExceptionThrown_ShouldThrowCreateLoanPaymentException()
        {
            // Arrange
            const int loanId = 1;
            const decimal paymentAmount = 100;
            
            var request = new CreateLoanPaymentCommand(new LoanPaymentRequestDto(loanId, paymentAmount));
            
      
            _validatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<LoanPaymentRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            
            _loanQuerySqlDbMock
                .Setup(q => q.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Loans, bool>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<Expression<Func<Loans, object>>>()))
                .ThrowsAsync(new Exception("Test exception"));
                
            _contextServiceMock
                .Setup(cs => cs.GetUserIdByToken())
                .Returns(1);

            // Act & Assert
            await Assert.ThrowsAsync<CreateLoanPaymentException>(() => 
                _handler.Handle(request, CancellationToken.None));
        }
    }
}
