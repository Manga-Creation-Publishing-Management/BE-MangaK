namespace Manga.Service.Auth;

public interface IService
{
    Task<Response.IdentityResponse> Login(Request request);
}