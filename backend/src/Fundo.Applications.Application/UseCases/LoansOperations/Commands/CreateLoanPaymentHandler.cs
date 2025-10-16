using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Apllication.Validators;
using Fundo.Applications.Application.Validators;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Enums;
using Fundo.Applications.Domain.Exceptions;
using Fundo.Applications.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace Fundo.Applications.Application.UseCases.LoansOperations.Commands;

public record LoanPaymentRequestDto(int LoanId,decimal Amount);
public record CreateLoanPaymentCommand(LoanPaymentRequestDto RequestDto) :IRequest<Response<LoanDto>>;
public class CreateLoanPaymentHandler(
    IQuerySqlDb<Loans> loanQuerySqlDb,
    ICommandSqlDb<Loans> loanCommandSqlDb,
    IContextService contextService,
    ILogger<CreateLoanPaymentHandler> logger,
    IValidator<LoanPaymentRequestDto> validator):IRequestHandler<CreateLoanPaymentCommand,Response<LoanDto>>
{
    public async Task<Response<LoanDto>> Handle(CreateLoanPaymentCommand request, CancellationToken cancellationToken)
    {


        try
        {
            var errorList = await ValidateRequest(request.RequestDto, cancellationToken);
            if (errorList.Any())
            {
                logger.LogError("Error validating loan creation request: {Errors}", string.Join(", ", errorList));
                return Response<LoanDto>.Fail(errorList);
            }

            var loan = await loanQuerySqlDb.FirstOrDefaultAsync(x => x.Id == request.RequestDto.LoanId, true,
                x => x.Applicant);
            Loans.DeductCurrentBalance(loan!, request.RequestDto.Amount);

            loanCommandSqlDb.UpdateAsync(loan!);
            await loanCommandSqlDb.SaveAsyncChanges(cancellationToken);

            var responseDto = new LoanDto(
                loan!.Id,
                loan.CurrentBalance,
                new UserDto(loan.Applicant.FirstName + " " + loan.Applicant.LastName),
                loan.CurrentBalance == 0 ? LoanStatusesEnum.PAID.ToString() : LoanStatusesEnum.ACTIVE.ToString()
            );
            return Response<LoanDto>.Success(responseDto, "Pago de prestamo registrado con exito");
        }catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing the loan payment for LoanId: {LoanId}", request.RequestDto.LoanId);
        throw new CreateLoanPaymentException(ex.Message);
        }

    }
    
    private async Task<List<string>> ValidateRequest(LoanPaymentRequestDto requestDto, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating loan payment request for LoanId: {LoanId} with Amount: {Amount}", requestDto.LoanId, requestDto.Amount);
        var result = await validator.ValidateAsync(requestDto, cancellationToken);
        return result.IsValid
            ? new List<string>()
            : result.Errors.Select(e => e.ErrorMessage).ToList();
    }
    

}