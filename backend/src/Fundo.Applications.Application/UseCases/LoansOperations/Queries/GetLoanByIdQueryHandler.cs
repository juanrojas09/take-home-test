using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Apllication.Validators;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fundo.Applications.Apllication.UseCases.LoansOperations.Queries;


public record GetLoanByIdQuery(int LoanId):IRequest<Response<Loans>>;

public class GetLoanByIdQueryHandler(ILogger<GetLoanByIdQueryHandler> logger,IContextService contextService,IQuerySqlDb<Loans> loanQuerySqlDb):IRequestHandler<GetLoanByIdQuery, Response<Loans>>
{
    public async Task<Response<Loans>> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
    {
        var validator= new GetLoanByIdValidator(contextService,loanQuerySqlDb);
        var validationResult=validator.Validate(request.LoanId);
        if (!validationResult.IsValid)
        {
            logger.LogError("There are validation errors for the GetLoanByIdQuery: {Errors}",string.Join(", ",validationResult.Errors.Select(e=>e.ErrorMessage)));
            var errors=validationResult.Errors.Select(x=>x.ErrorMessage).ToList();
            return Response<Loans>.Fail(errors,"Errores de validacion");
            
        }
        
        var loan=await loanQuerySqlDb.FirstOrDefaultAsync(x=>x.Id==request.LoanId,false,x=>x.Applicant,x=>x.Status);
        if(loan==null)
        {
            logger.LogError("Loan with Id {LoanId} not found",request.LoanId);
            return Response<Loans>.Fail(new List<string>{ "Loan not found" },"Loan not found");
        }
        return Response<Loans>.Success(loan,"Loan retrieved successfully");
    }
}