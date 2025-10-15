namespace Fundo.Applications.Apllication.Dtos;

public record RegisterUserRequestDto(string Email, string Password,string Name, string PhoneNumber, Guid TenantId,Guid RoleId);