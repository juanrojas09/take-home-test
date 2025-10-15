using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Apllication.UseCases.LoansOperations.Queries.GetLoans.Template;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fundo.Applications.Apllication.UseCases.LoansOperations.Queries.GetLoans;

public record GetAllLoansQuery(PaginationRequestDto PaginationRequestDto) : GetLoansQueryBase(PaginationRequestDto);

public class GetAllLoansQueryHandler(ILogger<GetLoansQueryHandlerBase> logger,IQuerySqlDb<Loans> loanQuerySqlDb,IContextService contextService): 
    GetLoansQueryHandlerBase( logger,loanQuerySqlDb, contextService)
{
}