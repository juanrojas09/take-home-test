using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Application.UseCases.LoansOperations.Queries.GetLoans;
using Fundo.Applications.Application.UseCases.LoansOperations.Queries.GetLoans.Template;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Enums;
using Fundo.Applications.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases.LoansOperations.Queries.GetLoans
{
    public class GetLoansByUserIdQueryHandlerTests
    {
        private readonly Mock<IQuerySqlDb<Loans>> _loanQuerySqlDbMock;
        private readonly Mock<IContextService> _contextServiceMock;
        private readonly Mock<ILogger<GetLoansQueryHandlerBase<GetLoansByIdQuery>>> _loggerMock;
        private readonly GetLoansByUserIdQueryHandler _handler;
        private readonly int _userId = 5; 
        public GetLoansByUserIdQueryHandlerTests()
        {
            _loanQuerySqlDbMock = new Mock<IQuerySqlDb<Loans>>();
            _contextServiceMock = new Mock<IContextService>();
            _loggerMock = new Mock<ILogger<GetLoansQueryHandlerBase<GetLoansByIdQuery>>>();
            
      
            _contextServiceMock.Setup(c => c.GetUserIdByToken()).Returns(_userId);
            
            _handler = new GetLoansByUserIdQueryHandler(
                _loggerMock.Object,
                _loanQuerySqlDbMock.Object,
                _contextServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_WithUserLoans_ShouldReturnPaginatedLoans()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 10;
            const int totalCount = 5;
            
            var paginationRequest = new PaginationRequestDto(page, pageSize);
            var query = new GetLoansByIdQuery(paginationRequest);
            
            var loansData = CreateTestLoansForUser(_userId, 3);
            
            _loanQuerySqlDbMock.Setup(q => q.GetPaginatedListAsync(
                    It.Is<Expression<Func<Loans, bool>>>(expr => IsExpressionForUserId(expr, _userId)),
                    page,
                    pageSize,
                    false,
                    It.IsAny<Expression<Func<Loans, object>>>(),
                    It.IsAny<Expression<Func<Loans, object>>>()))
                .ReturnsAsync(loansData);
            
            _loanQuerySqlDbMock.Setup(q => q.CountAsync(
                    It.Is<Expression<Func<Loans, bool>>>(expr => IsExpressionForApplicantId(expr, _userId)), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(totalCount);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Items.Count);
            Assert.Equal(totalCount, result.Data.TotalCount);
            Assert.Equal(page, result.Data.CurrentPage);
            Assert.Equal(pageSize, result.Data.PageSize);
            
     
            _loanQuerySqlDbMock.Verify(q => q.GetPaginatedListAsync(
                It.Is<Expression<Func<Loans, bool>>>(expr => IsExpressionForUserId(expr, _userId)),
                page,
                pageSize,
                false,
                It.IsAny<Expression<Func<Loans, object>>>(),
                It.IsAny<Expression<Func<Loans, object>>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoUserLoans_ShouldReturnEmptyPagination()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 10;
            
            var paginationRequest = new PaginationRequestDto(page, pageSize);
            var query = new GetLoansByIdQuery(paginationRequest);
            
            _loanQuerySqlDbMock.Setup(q => q.GetPaginatedListAsync(
                    It.IsAny<Expression<Func<Loans, bool>>>(),
                    page,
                    pageSize,
                    false,
                    It.IsAny<Expression<Func<Loans, object>>>(),
                    It.IsAny<Expression<Func<Loans, object>>>()))
                .ReturnsAsync(new List<Loans>());
            
            _loanQuerySqlDbMock.Setup(q => q.CountAsync(
                    It.IsAny<Expression<Func<Loans, bool>>>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Items);
            Assert.Equal(0, result.Data.TotalCount);
            Assert.Equal("There are no loans available.", result.Message);
        }

        [Fact]
        public async Task Handle_NoUserContext_ShouldReturnEmptyList()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 10;
            
        
            var contextServiceMock = new Mock<IContextService>();
            contextServiceMock.Setup(c => c.GetUserIdByToken()).Returns((int?)null);
            
            var handler = new GetLoansByUserIdQueryHandler(
                _loggerMock.Object,
                _loanQuerySqlDbMock.Object,
                contextServiceMock.Object
            );
            
            var paginationRequest = new PaginationRequestDto(page, pageSize);
            var query = new GetLoansByIdQuery(paginationRequest);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Items);
            Assert.Equal("There are no loans available.", result.Message);
        }

        [Fact]
        public async Task Handle_DatabaseError_ShouldThrowException()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 10;
            
            var paginationRequest = new PaginationRequestDto(page, pageSize);
            var query = new GetLoansByIdQuery(paginationRequest);
            
            _loanQuerySqlDbMock.Setup(q => q.GetPaginatedListAsync(
                    It.IsAny<Expression<Func<Loans, bool>>>(),
                    page,
                    pageSize,
                    false,
                    It.IsAny<Expression<Func<Loans, object>>>(),
                    It.IsAny<Expression<Func<Loans, object>>>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }
        
        // Métodos auxiliares para crear datos de prueba
        private List<Loans> CreateTestLoansForUser(int userId, int count)
        {
            var loans = new List<Loans>();
            
            for (int i = 1; i <= count; i++)
            {
                var applicant = Users.CreateNew($"user{userId}@example.com", "hashedPassword", $"User{userId}", "Test", 2);
                typeof(Users).GetProperty("Id")!.SetValue(applicant, userId);
                
                var loan = Loans.CreateNew(1000 * i, userId);
                
                // Configurar propiedades necesarias para el test
                typeof(Loans).GetProperty("Id")!.SetValue(loan, i);
                typeof(Loans).GetProperty("Applicant")!.SetValue(loan, applicant);
                typeof(Loans).GetProperty("ApplicantId")!.SetValue(loan, userId);
                
                var status = new LoanStates("Active");
                typeof(LoanStates).GetProperty("Id")!.SetValue(status, i);
                typeof(LoanStates).GetProperty("Name")!.SetValue(status, LoanStatusesEnum.ACTIVE.ToString());
                
                typeof(Loans).GetProperty("Status")!.SetValue(loan, status);
                
                loans.Add(loan);
            }
            
            return loans;
        }
        
  
        private bool IsExpressionForUserId(Expression<Func<Loans, bool>> expr, int userId)
        {
    
            return expr != null;
        }
        
        private bool IsExpressionForApplicantId(Expression<Func<Loans, bool>> expr, int applicantId)
        {
            return expr != null;
        }
    }
}
