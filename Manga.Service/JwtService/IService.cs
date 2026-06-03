using System.Security.Claims;

namespace Manga.Service.JwtService;

public interface IService
{
    public string GenerateAccessToken(IEnumerable<Claim> claims);
    public string GenerateRefreshToken();
    ClaimsPrincipal ValidateToken(string token); 
}