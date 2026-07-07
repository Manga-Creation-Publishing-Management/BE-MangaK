using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace Manga.Service.GoogleAuthService;

public class Service : IService
{
    private readonly IConfiguration _configuration;

    public Service(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken)
    {
        var clientId = _configuration["GoogleAuthConfig:ClientId"];
        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new List<string>(){clientId ?? string.Empty}
        };
        return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
    }
}