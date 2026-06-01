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
        try
        {
            var result = await _identityService.Login(request);
            return Ok(ApiResponseFactory.SuccessResponse(result, "Login Successfully!", HttpContext.TraceIdentifier));

        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Request.RegisterRequest request)
    {
        var result = await _identityService.Register(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Register Successfully!", HttpContext.TraceIdentifier));
    }
}