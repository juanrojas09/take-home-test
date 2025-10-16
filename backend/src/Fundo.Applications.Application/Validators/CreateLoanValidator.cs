using FluentValidation;
using Fundo.Applications.Application.UseCases.LoansOperations.Commands;

namespace Fundo.Applications.Application.Validators;

public class CreateLoanValidator:AbstractValidator<LoanRequestDto>
{
    public CreateLoanValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
    
}