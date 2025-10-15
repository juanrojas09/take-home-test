public async Task<Response<LoanDto>> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
{
    var validator = new CreateLoanValidator();
    if(!validator.Validate(request.requestDto).IsValid)
    {
        //return validation errors as JSON
        var errors = validator.Validate(request.requestDto).Errors.Select(e => e.ErrorMessage).ToList();
        return Response<LoanDto>.Fail(errors, "Validation errors occurred");
    }
    
    // Resto del método...
}

