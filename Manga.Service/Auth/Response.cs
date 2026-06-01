using Manga.Repository.Entity;

namespace Manga.Service.Auth;

public class Response
{
    public class AuthResponse
    {
        public Guid UserId { get; set;}
        public string Email { get; set;} = string.Empty;
        public string FullName { get; set;} = string.Empty;
        public string Role { get; set;} = string.Empty;
        public string? Phone  { get; set;} = string.Empty; 
        public string AccessToken { get; set;} = string.Empty;
        // public string RefreshToken { get; set; } = string.Empty;
    }
}