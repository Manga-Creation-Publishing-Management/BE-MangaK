using System.Security.Claims;
using Manga.Repository.Data;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.Auth;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly JwtService.IService _jwtService;

    public Service(AppDbContext dbContext, JwtService.IService jwtService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
    }

    public async Task<Response.AuthResponse> Login(Request.LoginRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.Verified)// dòng này kiểm tra xem user này đã xác thực email chưa kiểu dữ liệu là bool
            throw new InvalidOperationException("Account is not verified.");
        
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var claim = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Email", user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        
        var accessToken = _jwtService.GenerateAccessToken(claim);
        
        return new Response.AuthResponse()
        {
            UserId = user.Id,
            Email =  user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            AccessToken = accessToken
            // RefreshToken = refreshToken,
        };
    }

}