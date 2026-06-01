using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Manga.Service.JwtService;

public class Service : IService
{
    private readonly JwtOption _jwtOption = new();

    public Service(IConfiguration configuration)
    {
        configuration.GetSection("JwtOptions").Bind(_jwtOption);
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOption.SecretKey));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var tokenOptions = new JwtSecurityToken(
            issuer: _jwtOption.Issuer,
            audience: _jwtOption.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtOption.Expiration),
            signingCredentials: signingCredentials);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        return tokenString;
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        throw new NotImplementedException();
    }
}