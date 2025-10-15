using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Fundo.Applications.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Fundo.Applications.Domain.Common;

public class Jwt
{

 public static string GenerateJwt(Users user,string secretKey)
 {
  var email = user?.Email ?? string.Empty;
  var userId = user?.Id.ToString() ?? string.Empty;
  var roleName = user?.Role?.Name ?? string.Empty;
  
  List<Claim> claims = new List<Claim>()
  {
   new(ClaimTypes.Email, email),
   new(ClaimTypes.Name, user?.FirstName + "" + user?.LastName ?? string.Empty),
   new(ClaimTypes.NameIdentifier, userId),
   new(ClaimTypes.Role, roleName),
  };

  var key=new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
  var creds=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
  
  var token=new JwtSecurityToken(
   claims:claims,
   expires:DateTime.Now.AddHours(8),
   signingCredentials:creds
  );

  return new JwtSecurityTokenHandler().WriteToken(token);
 }
 
 
 public static bool ValidateToken(string token,string secretKey)
 {
  var tokenHandler = new JwtSecurityTokenHandler();
  var key = System.Text.Encoding.UTF8.GetBytes(secretKey);
  try{
   
   tokenHandler.ValidateToken(token,new TokenValidationParameters()
   {
    ValidateIssuerSigningKey=true,
    IssuerSigningKey=new SymmetricSecurityKey(key),
    ValidateIssuer=false,
    ValidateAudience=false,
    ClockSkew=TimeSpan.Zero
   },out _);

   return true;
  }catch(Exception ex)
  {
   return false;
  }
 
 }
 
 
 public static List<ClaimsPrincipal> GetTokenClaims(string token)
 {
  var handler = new JwtSecurityTokenHandler();
  var jwtSecurityToken = handler.ReadJwtToken(token);
  var identity = new ClaimsIdentity(jwtSecurityToken.Claims);
  var principal = new ClaimsPrincipal(identity);
  return new List<ClaimsPrincipal> { principal };
 }
 

    
}