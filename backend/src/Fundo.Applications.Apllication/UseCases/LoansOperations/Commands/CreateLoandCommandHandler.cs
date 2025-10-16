public async Task<Response<LoanDto>> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
{
    var validator = new CreateLoanValidator();
    var result = await validator.ValidateAsync(request.requestDto, cancellationToken);
    if(!result.IsValid)
    {
        //return validation errors as JSON
        var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
        return Response<LoanDto>.Fail(errors, "Validation errors occurred");
    }
    
    // Resto del método...
}

