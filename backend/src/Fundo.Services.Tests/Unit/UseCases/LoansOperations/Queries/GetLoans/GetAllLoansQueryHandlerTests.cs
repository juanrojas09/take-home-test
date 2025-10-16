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
    public class GetAllLoansQueryHandlerTests
    {
        private readonly Mock<IQuerySqlDb<Loans>> _loanQuerySqlDbMock;
        private readonly Mock<IContextService> _contextServiceMock;
        private readonly Mock<ILogger<GetLoansQueryHandlerBase<GetAllLoansQuery>>> _loggerMock;
        private readonly GetAllLoansQueryHandler _handler;

        public GetAllLoansQueryHandlerTests()
        {
            _loanQuerySqlDbMock = new Mock<IQuerySqlDb<Loans>>();
            _contextServiceMock = new Mock<IContextService>();
            _loggerMock = new Mock<ILogger<GetLoansQueryHandlerBase<GetAllLoansQuery>>>();
            
            _handler = new GetAllLoansQueryHandler(
                _loggerMock.Object,
                _loanQuerySqlDbMock.Object,
                _contextServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_WithLoans_ShouldReturnPaginatedLoans()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 10;
            const int totalCount = 15;
            
            var paginationRequest = new PaginationRequestDto(page, pageSize);
            var query = new GetAllLoansQuery(paginationRequest);
            
            var loansData = CreateTestLoans(3); 
            
            _loanQuerySqlDbMock.Setup(q => q.GetPaginatedListAsync(
                    It.IsAny<Expression<Func<Loans, bool>>>(),
                    page,
                    pageSize,
                    false,
                    It.IsAny<Expression<Func<Loans, object>>>(),
                    It.IsAny<Expression<Func<Loans, object>>>()))
                .ReturnsAsync(loansData);
            
            _loanQuerySqlDbMock.Setup(q => q.CountAsync(It.IsAny<Expression<Func<Loans, bool>>>(), It.IsAny<CancellationToken>()))
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
                It.IsAny<Expression<Func<Loans, bool>>>(),
                page,
                pageSize,
                false,
                It.IsAny<Expression<Func<Loans, object>>>(),
                It.IsAny<Expression<Func<Loans, object>>>()), Times.Once);
            
            _loanQuerySqlDbMock.Verify(q => q.CountAsync(It.IsAny<Expression<Func<Loans, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoLoans_ShouldReturnEmptyPagination()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 10;
            
            var paginationRequest = new PaginationRequestDto(page, pageSize);
            var query = new GetAllLoansQuery(paginationRequest);
            
            _loanQuerySqlDbMock.Setup(q => q.GetPaginatedListAsync(
                    It.IsAny<Expression<Func<Loans, bool>>>(),
                    page,
                    pageSize,
                    false,
                    It.IsAny<Expression<Func<Loans, object>>>(),
                    It.IsAny<Expression<Func<Loans, object>>>()))
                .ReturnsAsync(new List<Loans>());
            
            _loanQuerySqlDbMock.Setup(q => q.CountAsync(It.IsAny<Expression<Func<Loans, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Items);
            Assert.Equal(0, result.Data.TotalCount);
            Assert.Equal(page, result.Data.CurrentPage);
            Assert.Equal(pageSize, result.Data.PageSize);
            Assert.Equal("There are no loans available.", result.Message);
        }

        [Fact]
        public async Task Handle_DatabaseError_ShouldThrowException()
        {
            // Arrange
            const int page = 1;
            const int pageSize = 10;
            
            var paginationRequest = new PaginationRequestDto(page, pageSize);
            var query = new GetAllLoansQuery(paginationRequest);
            
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
        
        //Helper to arrange test data
        private List<Loans> CreateTestLoans(int count)
        {
            var loans = new List<Loans>();
            
            for (int i = 1; i <= count; i++)
            {
                var applicant = Users.CreateNew($"user{i}@example.com", "hashedPassword", $"User{i}", "Test", 2);
                var loan = Loans.CreateNew(1000 * i, i);
                
                typeof(Loans).GetProperty("Id")!.SetValue(loan, i);
                typeof(Loans).GetProperty("Applicant")!.SetValue(loan, applicant);
                
                var status = new LoanStates("Active");
                typeof(LoanStates).GetProperty("Id")!.SetValue(status, i);
                typeof(LoanStates).GetProperty("Name")!.SetValue(status, LoanStatusesEnum.ACTIVE.ToString());
                
                typeof(Loans).GetProperty("Status")!.SetValue(loan, status);
                
                loans.Add(loan);
            }
            
            return loans;
        }
    }
}
