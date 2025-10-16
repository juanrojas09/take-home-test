using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Application.UseCases.LoansOperations.Queries.GetLoans.Template;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fundo.Applications.Application.UseCases.LoansOperations.Queries.GetLoans;

// Query para obtener todos los préstamos
public record GetAllLoansQuery(PaginationRequestDto PaginationRequestDto) : GetLoansQueryBase(PaginationRequestDto);

// Implementación del handler para GetAllLoansQuery
public class GetAllLoansQueryHandler : 
    GetLoansQueryHandlerBase<GetAllLoansQuery>
{
    private readonly ILogger<GetLoansQueryHandlerBase<GetAllLoansQuery>> _logger;
    private readonly IQuerySqlDb<Loans> _loanQuerySqlDb;
    private readonly IContextService _contextService;

    public GetAllLoansQueryHandler(
        ILogger<GetLoansQueryHandlerBase<GetAllLoansQuery>> logger,
        IQuerySqlDb<Loans> loanQuerySqlDb,
        IContextService contextService) : base(logger, loanQuerySqlDb, contextService)
    {
        _logger = logger;
        _loanQuerySqlDb = loanQuerySqlDb;
        _contextService = contextService;
    }

    // Reutiliza la lógica base llamando al método base
    public async Task<Response<Pagination<LoanDto>>> Handle(GetAllLoansQuery request, CancellationToken cancellationToken)
    {
        return await base.Handle(request, cancellationToken);
    }
}