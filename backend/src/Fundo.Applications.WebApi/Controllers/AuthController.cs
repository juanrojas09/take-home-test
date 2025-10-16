using Fundo.Applications.Apllication.UseCases.Authentication.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Fundo.Applications.Application.UseCases.Authentication.Queries;

namespace Fundo.Applications.WebApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginDto)
        {
            var query = new LoginUserQuery(loginDto);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    403 => StatusCode(403, result),
                    _ => BadRequest(result)
                };
            }

            return Ok(result);
        }

   
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto registerDto)
        {
            var command = new RegisterUserCommand(registerDto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return StatusCode(201, result);
        }
    }
}
