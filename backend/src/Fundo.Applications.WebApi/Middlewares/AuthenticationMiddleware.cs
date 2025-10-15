using System;
using System.Linq;
using System.Threading.Tasks;
using Fundo.Applications.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Fundo.Applications.WebApi.Middlewares;

public class AuthenticationMiddleware(IConfiguration configuration):IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        
        var token=context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

       
        var tokenWithoutBearer=token?.Replace("Bearer ","");
        if (string.IsNullOrEmpty(tokenWithoutBearer))
        {
            tokenWithoutBearer = "";
        }

        var secretKey=configuration.GetSection("JWT").GetValue<string>("Key");
         
        if (
            string.IsNullOrEmpty(tokenWithoutBearer) 
            && !Jwt.ValidateToken(tokenWithoutBearer, secretKey)
            && !isAuthEndpoint(context)
        )
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(Response<string>.Unauthorized()));
            return;
             
             
        };
         
        await next(context);

    }
    
    
    public bool isAuthEndpoint(HttpContext context)
    {
        var path=context.Request.Path.Value;
        if (path.Contains("auth", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
         
        return false;
     
    }
}