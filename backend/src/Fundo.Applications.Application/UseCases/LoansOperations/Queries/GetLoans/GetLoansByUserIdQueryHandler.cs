using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Apllication.UseCases.LoansOperations.Queries.GetLoans.Template;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fundo.Applications.Apllication.UseCases.LoansOperations.Queries.GetLoans;

public record GetLoansByIdQuery(PaginationRequestDto PaginationRequestDto) : GetLoansQueryBase(PaginationRequestDto);

public class GetLoansByUserIdQueryHandler(ILogger<GetLoansQueryHandlerBase> logger,IQuerySqlDb<Loans> loanQuerySqlDb, IContextService contextService) 
    : GetLoansQueryHandlerBase(logger,loanQuerySqlDb, contextService)
{
    internal override async Task<List<Loans>> FetchLoansAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var userId = contextService.GetUserIdByToken();
        if (userId == null) return new List<Loans>();

        return await loanQuerySqlDb.GetPaginatedListAsync(
            x => x.Applicant.Id == userId,
            page,
            pageSize,
            false,
            x => x.Applicant,
            x => x.Status);
    }
}