using Manga.Service.Auth;
using Manga.Service.Model;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{   
    private readonly IService _identityService;
    public AuthController(IService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Request.LoginRequest request)
    {
            var result = await _identityService.Login(request);
            return Ok(ApiResponseFactory.SuccessResponse(result, "Login Successfully!", HttpContext.TraceIdentifier));
    }
}