using Google.Apis.Auth;

namespace Manga.Service.GoogleAuthService;

public interface IService
{
    Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken);
}