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

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] Request.ForgotPasswordRequest request)
    {
        var result = await _identityService.ForgotPassword(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Please check Mail", HttpContext.TraceIdentifier));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] Request.ChangePasswordRequest request)
    {
        var result = await _identityService.ChangePassword(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Change password successfully", HttpContext.TraceIdentifier));
    }
}