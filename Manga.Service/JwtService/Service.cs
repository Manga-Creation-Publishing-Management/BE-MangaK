using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Manga.Service.JwtService;

public class Service : IService
{

    private readonly JwtOptions _jwtOptions = new();
    public Service(IConfiguration configuration)
    {
        configuration.GetSection("JwtOptions").Bind(_jwtOptions);
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var tokenOptions = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtOptions.ExpireMinutes),
            signingCredentials: signingCredentials);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        return tokenString;
    }

    public string GenerateRefreshToken()
    {
        var bytes =  new byte[64];
        using var randomNumber = RandomNumberGenerator.Create();
        randomNumber.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        throw new NotImplementedException();
    }
}