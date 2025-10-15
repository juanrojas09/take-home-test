using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Apllication.Validators;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Enums;
using Fundo.Applications.Domain.Exceptions;
using Fundo.Applications.Domain.Interfaces;
using MediatR;

namespace Fundo.Applications.Apllication.UseCases.LoansOperations.Commands;
//apply vertical slice on handlers and query to bring balance on file number and lines of code

public record LoanRequestDto(decimal Amount);
public record CreateLoanCommand(LoanRequestDto requestDto) : IRequest<Response<LoanDto>>;

public class CreateLoanCommandHandler(ICommandSqlDb<Loans> loanCommandSqlDb,IContextService contextService):IRequestHandler<CreateLoanCommand,Response<LoanDto>>
{
    /// <summary>
    /// Handler that creates a new loan for the authenticated user.
    /// Validates the request, retrieves the user ID from the context, maps the request to a Loan entity, and saves it to the database.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Response<LoanDto>> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
    {
        
        var errors = ValidateRequest(request.requestDto);
        if (errors.Any())
            return Response<LoanDto>.Fail(errors, "Validation errors occurred");


        var userId = contextService.GetUserIdByToken();
        if (userId == null)
        {
            return Response<LoanDto>.Fail("User is not valid.");
        }
        
        var loan = MapToEntity(request, (int)userId);
        await loanCommandSqlDb.AddAsync(loan);
        await loanCommandSqlDb.SaveAsyncChanges(cancellationToken);
        
        var applicantName= contextService.GetUserNameByToken() ?? "N/A";
        
        var response=new LoanDto(loan.Id,loan.Amount,new UserDto( applicantName),LoanStatusesEnum.ACTIVE.ToString());
        
        return Response<LoanDto>.Success(response,"Loan created successfully");


    }
    
    
    
    private List<string> ValidateRequest(LoanRequestDto requestDto)
    {
        var validator = new CreateLoanValidator();
        var result = validator.Validate(requestDto);
        return result.IsValid ? new List<string>() : result.Errors.Select(e => e.ErrorMessage).ToList();
    }
    
  
    private Loans MapToEntity(CreateLoanCommand request, int userId)
    {
       return Loans.CreateNew(request.requestDto.Amount,(int)userId);
    }
}