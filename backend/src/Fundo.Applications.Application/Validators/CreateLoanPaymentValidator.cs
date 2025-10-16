using FluentValidation;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Application.UseCases.LoansOperations.Commands;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;

namespace Fundo.Applications.Application.Validators;

public class CreateLoanPaymentValidator:AbstractValidator<LoanPaymentRequestDto>
{
    public CreateLoanPaymentValidator(IQuerySqlDb<Loans> querySqlDb, IContextService contextService)
    {

        RuleFor(x => x.Amount)
            .GreaterThan(0m).WithMessage("Amount must be greater than zero.");
          

     

        RuleFor(x => x.LoanId)
            .MustAsync(async (loanId, ct) => await querySqlDb.WhereAsync(x => x.Id == loanId) != null)
            .WithMessage("Loan not found.");

      
        RuleFor(x => x)
            .MustAsync(async (dto, ct) =>
            {
                var loan = await querySqlDb.FirstOrDefaultAsync(x=>x.Id == dto.LoanId,false,x=>x.Applicant);
                if (loan == null) return false;
                var userId = contextService.GetUserIdByToken();
                if (userId == null) return false;
                return loan.Applicant.Id == userId;
            })
            .WithMessage("Loan not found for the user.");

        
        RuleFor(x => x)
            .MustAsync(async (dto, ct) =>
            {
                var outstanding = await  querySqlDb.FirstOrDefaultAsync(x=>x.Id == dto.LoanId,false);
                if (outstanding == null) return false;
                return dto.Amount <= outstanding.CurrentBalance;
               
            })
            .WithMessage("Amount exceeds the outstanding balance.");
    }
}