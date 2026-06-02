using System.Security.Claims;
using Manga.Repository.Data;
using Manga.Repository.Entity;
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

    public async Task<Response.LoginResponse> Login(Request.LoginRequest request)
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
        var refreshToken = _jwtService.GenerateRefreshToken();

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            DeviceFingerprint = request.DeviceFingerprint ?? "Unknown",
            RefreshToken = refreshToken,
            ExpiresAt =  DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt =  DateTime.UtcNow
        };
        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync();
        
        return new Response.LoginResponse()
        {
            UserId = user.Id,
            Email =  user.Email,
            // FullName = $"{user.FirstName} {user.LastName}",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            // Phone =  user.Phone,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<Response.RegistrationResponse> Register(Request.RegisterRequest request)
    {
        var emailExist = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExist)
        {
            throw new ArgumentException("Email already exists");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required");
        }

        if (request.Password.Length < 6)
        {
            throw new ArgumentException("Password is too short.");
        }
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName =  request.LastName,
            Email =  request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone =  request.Phone ?? "",
            Role = request.Role,
            Verified = true,
            Status = request.Status,
            CreatedAt =  DateTime.UtcNow
        };
        _dbContext.Add(user);
        await _dbContext.SaveChangesAsync();
        return new Response.RegistrationResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString()
        };
    }

    public Task<Response.RefreshTokenResponse> RefreshToken(Request.RefreshTokenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Response.ForgotPasswordResponse> ForgotPassword(Request.ForgotPasswordRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Response.ChangePasswordResponse> ChangePassword(Request.ChangePasswordRequest request)
    {
        throw new NotImplementedException();
    }
}