using FluentValidation;
using Fundo.Applications.Apllication.UseCases.LoansOperations.Commands;

namespace Fundo.Applications.Apllication.Validators;

public class CreateLoanValidator:AbstractValidator<LoanRequestDto>
{
    public CreateLoanValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
    
}