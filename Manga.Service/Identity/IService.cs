namespace Manga.Service.Identity;

public interface IService
{
    Task<Response.IdentityResponse> Login(LoginRequest request);
}