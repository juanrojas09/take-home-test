using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;
using MediatR;

namespace Fundo.Applications.Apllication.UseCases.Authentication.Commands;

public record RegisterUserRequestDto(string Email, string Password,string Name, string LastName,int RoleId);
public record RegisterUserCommand(RegisterUserRequestDto RegisterUserRequestDto) : IRequest<Response<bool>>;

public class RegisterUserCommandHandler(ICommandSqlDb<Users>usersRepository):IRequestHandler<RegisterUserCommand, Response<bool>>
{
    public async Task<Response<bool>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var hashedPassword=Users.GeneratePasswordHash(request.RegisterUserRequestDto.Password);

        var user = Users.CreateNew(
            request.RegisterUserRequestDto.Email,
            hashedPassword,
            request.RegisterUserRequestDto.Name,
            request.RegisterUserRequestDto.LastName,
            request.RegisterUserRequestDto.RoleId
        );
  
       
        await usersRepository.AddAsync(user);
        await usersRepository.SaveAsyncChanges(cancellationToken);
       
        return Response<bool>.Success(true,"User registered successfully");

    }
}