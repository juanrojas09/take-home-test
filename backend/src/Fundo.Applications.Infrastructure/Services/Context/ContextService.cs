using System.Security.Claims;
using Fundo.Applications.Apllication.Interfaces;
using Fundo.Applications.Domain.Common;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver.Linq;

namespace Fundo.Applications.Infrastructure.Services.Context;

public class ContextService(IHttpContextAccessor httpContextAccessor):IContextService
{
    public int? GetUserIdByToken()
    {
       var tokenHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
       if (string.IsNullOrEmpty(tokenHeader) || !tokenHeader.StartsWith("Bearer "))
       {
           return null;
       }
       var tokenWithoutBearer = tokenHeader.Replace("Bearer ", "");
       var claims = Jwt.GetTokenClaims(tokenWithoutBearer);
       if (claims.Any())
       {
           var userIdClaim = claims?.Where(x => x.FindFirst(ct => ct.Type == ClaimTypes.NameIdentifier) != null)
               .Select(claim=>claim.FindFirst(c=>c.Type == ClaimTypes.NameIdentifier)).First();
           return int.Parse(userIdClaim?.Value??"0");
       }
       return null;

    }

    public string? GetUserNameByToken()
    {
        var tokenHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(tokenHeader) || !tokenHeader.StartsWith("Bearer "))
        {
            return null;
        }
        var tokenWithoutBearer = tokenHeader.Replace("Bearer ", "");
        var claims = Jwt.GetTokenClaims(tokenWithoutBearer);
        if (claims.Any())
        {
            var userFullNameClaim = claims?.Where(x => x.FindFirst(ct => ct.Type == ClaimTypes.Name) != null)
                .Select(claim=>claim.FindFirst(c=>c.Type == ClaimTypes.Name)).First();
            return userFullNameClaim?.Value;
        }
        return null;
    }

    public string? GetCustomerRoleByToken()
    {
        var tokenHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(tokenHeader) || !tokenHeader.StartsWith("Bearer "))
        {
            return null;
        }
        var tokenWithoutBearer = tokenHeader.Replace("Bearer ", "");
        var claims = Jwt.GetTokenClaims(tokenWithoutBearer);
        if (claims.Any())
        {
            var userRoleName = claims?.Where(x => x.FindFirst(ct => ct.Type == ClaimTypes.Role) != null)
                .Select(claim=>claim.FindFirst(c=>c.Type == ClaimTypes.Role)).First();
            return userRoleName?.Value;
        }
        return null;
    }
}