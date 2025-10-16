using System;
using System.Threading.Tasks;
using Fundo.Applications.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Fundo.Applications.WebApi.Middlewares;

public class AuthenticationMiddleware(IConfiguration configuration):IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {


        var cookieValue = context.Request.Cookies["access_token"];


        var tokenWithoutBearer = cookieValue;
        if (!string.IsNullOrEmpty(tokenWithoutBearer) && tokenWithoutBearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            tokenWithoutBearer = tokenWithoutBearer.Substring("Bearer ".Length);
        }
        if (string.IsNullOrEmpty(tokenWithoutBearer))
        {
            tokenWithoutBearer = "";
        }
        var secretKey = configuration.GetSection("JWT").GetValue<string>("Key");


        if (!IsAuthEndpoint(context) && (string.IsNullOrEmpty(tokenWithoutBearer) || !Jwt.ValidateToken(tokenWithoutBearer, secretKey)))
       {
            context.Response.StatusCode = 403;
           context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(Response<string>.Unauthorized()));
            return;
       }

      await next(context);



    }
    
    
    public bool IsAuthEndpoint(HttpContext context)
    {
        var path=context.Request.Path.Value;
        if (path.Contains("auth", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
         
        return false;
     
    }
}