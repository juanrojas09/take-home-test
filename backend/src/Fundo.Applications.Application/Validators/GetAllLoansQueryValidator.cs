using FluentValidation;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Apllication.UseCases.LoansOperations.Queries;
using Fundo.Applications.Apllication.UseCases.LoansOperations.Queries.GetLoans;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;

namespace Fundo.Applications.Apllication.Validators;

public class GetAllLoansQueryValidator:AbstractValidator<GetAllLoansQuery>
{
    
    public GetAllLoansQueryValidator(IContextService contextService)
    {
        RuleFor(x => x.PaginationRequestDto.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");
        
        RuleFor(x => x.PaginationRequestDto.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must be less than or equal to 100");
        
    }
}