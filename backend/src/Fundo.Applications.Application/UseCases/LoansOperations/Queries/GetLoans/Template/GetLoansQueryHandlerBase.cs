using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fundo.Applications.Apllication.UseCases.LoansOperations.Queries.GetLoans.Template;

public record GetLoansQueryBase(PaginationRequestDto PaginationRequestDto) : IRequest<Response<Pagination<LoanDto>>>;

public abstract class GetLoansQueryHandlerBase(ILogger<GetLoansQueryHandlerBase>logger,IQuerySqlDb<Loans> loanQuerySqlDb, IContextService contextService)
    : IRequestHandler<GetLoansQueryBase, Response<Pagination<LoanDto>>>
{

    /// <summary>
    /// Fetch all user loans with pagination.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Response<Pagination<LoanDto>>> Handle(GetLoansQueryBase request, CancellationToken cancellationToken)
    {
        var page = request.PaginationRequestDto.PageNumber;
        var pageSize = request.PaginationRequestDto.PageSize;

        var loans = await FetchLoansAsync(page, pageSize, cancellationToken);

        if (loans == null || loans.Count == 0)
        {
            logger.LogInformation("No loans found in the database.");
            return BuildEmptyResponse(page, pageSize, "There are no loans available.");
        }

        var loansDto = MapToDtoList(loans);

        var pagination = await BuildPaginationAsync(loansDto, page, pageSize, cancellationToken);

        return Response<Pagination<LoanDto>>.Success(pagination);
    }
    
    
    #region Private Methods

    internal virtual async Task<List<Loans>> FetchLoansAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching loans from database. Page: {Page}, PageSize: {PageSize}", page, pageSize);
        return await loanQuerySqlDb.GetPaginatedListAsync(
            x => true,
            page,
            pageSize,
            false,
            x => x.Applicant,
            x => x.Status);
    }

    private Response<Pagination<LoanDto>> BuildEmptyResponse(int page, int pageSize, string message)
    {
        var empty = new Pagination<LoanDto>
        {
            Items = new List<LoanDto>(),
            TotalCount = 0,
            CurrentPage = page,
            PageSize = pageSize
        };
        return Response<Pagination<LoanDto>>.Success(empty, message);
    }

    private List<LoanDto> MapToDtoList(IEnumerable<Loans> loans)
    {
        return loans.Select(x =>
                new LoanDto(
                    x.Id,
                    x.CurrentBalance,
                    new UserDto(x.Applicant.FirstName + " " + x.Applicant.LastName),
                    x.Status.Name))
            .ToList();
    }

    private async Task<Pagination<LoanDto>> BuildPaginationAsync(List<LoanDto> items, int page, int pageSize, CancellationToken cancellationToken)
    {
        var total = await loanQuerySqlDb.CountAsync(x => true, cancellationToken);
        return new Pagination<LoanDto>
        {
            Items = items,
            TotalCount = total,
            CurrentPage = page,
            PageSize = pageSize
        };
    }
    #endregion


}