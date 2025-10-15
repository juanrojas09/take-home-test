using FluentValidation;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;

namespace Fundo.Applications.Apllication.Validators;

public class GetLoanByIdValidator:AbstractValidator<int>
{
    public GetLoanByIdValidator(IContextService contextService, IQuerySqlDb<Loans> loanQuery)
    {
        RuleFor(x => x).GreaterThan(0).WithMessage("Loan ID must be greater than zero.");
        
    
        RuleFor(x => x).MustAsync(async (id, cancellation) =>
        {
            var loan = await loanQuery.FirstOrDefaultAsync(x => x.Id == id);
            return loan != null;
        }).WithMessage("Loan not found for the given ID.");

        RuleFor(x => x).MustAsync(async (id, cancellation) =>
        {
            var userId = contextService.GetUserIdByToken();
            var loan = await loanQuery.FirstOrDefaultAsync(x => x.Id == id && x.ApplicantId == userId);
            return loan != null;
        }).WithMessage("You don't have permission to access this loan.");
        
        
    }
}