using Fundo.Applications.Apllication.Dtos;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Fundo.Applications.Application.UseCases.Authentication.Queries;

public record LoginUserDto(string Email, string Password);

public record LoginUserResponseDto(string Token);

public record LoginUserQuery(LoginUserDto LoginUserDto) : IRequest<Response<LoginUserResponseDto>>;

public class LoginUserQueryHandler(IQuerySqlDb<Users> userRepository,IConfiguration configuration):IRequestHandler<LoginUserQuery, Response<LoginUserResponseDto>>
{
    public async Task<Response<LoginUserResponseDto>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
       
       
        var user = await userRepository.FirstOrDefaultAsNoTrackingAsync(x => x.Email == request.LoginUserDto.Email,false,x=>x.Role);
        if (user == null)
        {
            return Response<LoginUserResponseDto>.NotFound("The user with the provided email does not exist.");
        }


        if (!Users.ValidateLoginPassword(request.LoginUserDto.Password, user.Password))
        {
            return Response<LoginUserResponseDto>.Forbidden("Invalid Credentials.");
        }

        var secretKey = configuration.GetSection("JWT")["Key"];
        var jwt = Jwt.GenerateJwt(user, secretKey!);
        return Response<LoginUserResponseDto>.Success(new LoginUserResponseDto(jwt), "Successfull login");
        


    }
    



}