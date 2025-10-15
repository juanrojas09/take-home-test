namespace Fundo.Applications.Apllication.Dtos;

public record LoanDto(
    int Id,
    decimal CurrentBalance,
    UserDto Applicant,
    string Status
);


