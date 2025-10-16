using System;
using Fundo.Applications.Apllication.UseCases.Authentication.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Fundo.Applications.Application.UseCases.Authentication.Queries;
using Microsoft.AspNetCore.Http;

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
            
            var cookieOptions = new CookieOptions {
                HttpOnly = true,
                Secure = true,            
                SameSite = SameSiteMode.Strict, 
                Expires = DateTime.UtcNow.AddMinutes(60),
                Path = "/"
            };
            Response.Cookies.Append("access_token", result.Data.Token, cookieOptions);

            return Ok(result);
        }
        
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            if (Request.Cookies.ContainsKey("access_token"))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(-1), 
                    Path = "/"
                };
                Response.Cookies.Append("access_token", "", cookieOptions);
            }

            return Ok(new { Message = "Successfully logged out." });
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
