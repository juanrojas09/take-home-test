using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Application.UseCases.LoansOperations.Queries.GetLoans.Template;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Fundo.Applications.Application.UseCases.LoansOperations.Queries.GetLoans;

public record GetLoansByIdQuery(PaginationRequestDto PaginationRequestDto) : GetLoansQueryBase(PaginationRequestDto);

public class GetLoansByUserIdQueryHandler(ILogger<GetLoansQueryHandlerBase<GetLoansByIdQuery>> logger,
    IQuerySqlDb<Loans> loanQuerySqlDb, IContextService contextService)
    : GetLoansQueryHandlerBase<GetLoansByIdQuery>(logger, loanQuerySqlDb, contextService)
{
    private int? userId = contextService.GetUserIdByToken();
    internal override async Task<List<Loans>> FetchLoansAsync(int page, int pageSize, CancellationToken cancellationToken)
    {

        if (userId == null) return new List<Loans>();

        return await loanQuerySqlDb.GetPaginatedListAsync(
            x => x.Applicant.Id == userId,
            page,
            pageSize,
            false,
            x => x.Applicant,
            x => x.Status);
    }
    internal override async Task<Pagination<LoanDto>> BuildPaginationAsync(List<LoanDto> items, int page, int pageSize, CancellationToken cancellationToken)
    {
        var total = await loanQuerySqlDb.CountAsync(x => x.ApplicantId==userId, cancellationToken);
        return new Pagination<LoanDto>
        {
            Items = items,
            TotalCount = total,
            CurrentPage = page,
            PageSize = pageSize
        };
    }
}