using System.Security.Claims;
using Manga.Repository.Data;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.Auth;

public class Service : IService
{
    private readonly AppDbContext _db;
    private readonly JwtService.IService _jwtService;


    public Service(AppDbContext db, JwtService.IService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    public async Task<Response.IdentityResponse> Login(Request request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (user == null)
        {
            throw new Exception("Invalid email");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.PasswordHash, user.PasswordHash);
        if (isPasswordValid == false)
        {
            throw new Exception("Invalid password");
        }

        var claim = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Email", user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var token = _jwtService.GenerateAccessToken(claim);
        var result = new Response.IdentityResponse
        {
            AccessToken = token
        };
        return result;
    }

    public Task<Response.IdentityResponse> Register(Request request)
    {
        throw new NotImplementedException();
    }
}