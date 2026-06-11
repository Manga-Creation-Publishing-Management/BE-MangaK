using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;

namespace Manga.Service.Auth;

public class Request
{
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? DeviceFingerprint { get; set; }
    }

    public class RegisterRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? Phone { get; set; }
        public string? AuthorName { get; set; }
        public UserStatus Status { get; set; }
    }

    public class RegisterReaderRequest
    {
        public required string Email { get; set; }
    }

    public class RefreshTokenRequest
    {
        public required string RefreshToken { get; set; }
        public required string DeviceFingerprint { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public required string Email { get; set;} = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public required int Code { get; set; }
        public required string NewPassword { get; set;} = string.Empty;
    }

    public class LogoutRequest
    {
        public required string refreshToken { get; set; }
    }
}