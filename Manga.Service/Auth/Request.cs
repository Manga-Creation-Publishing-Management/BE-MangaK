namespace Manga.Service.Auth;

public class Request
{
    public string Email { get; set; } =  string.Empty;
    public string PasswordHash { get; set; } = string.Empty; 
}