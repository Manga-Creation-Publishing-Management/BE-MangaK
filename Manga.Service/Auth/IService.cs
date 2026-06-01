namespace Manga.Service.Auth;

public interface IService
{
    Task<Response.AuthResponse> Login(Request.LoginRequest request);
}