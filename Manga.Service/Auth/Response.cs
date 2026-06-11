using Manga.Repository.Entity.Enums;

namespace Manga.Service.Auth;

public class Response
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        // public string? Phone  { get; set;} = string.Empty; 
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RegistrationResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class RegisterReaderResponse
    {
    }
}