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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Request.RegisterRequest request)
    {
        var result = await _identityService.Register(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Register Successfully!", HttpContext.TraceIdentifier));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] Request.LogoutRequest request, CancellationToken cancellationToken)
    {
        var isSuccess = await _identityService.Logout(request, cancellationToken);
        if (!isSuccess)
        {

            return Ok(ApiResponseFactory.ErrorResponse( "Logout failed!", "The session does not exist, has expired, or has already been revoked.",
                traceId: HttpContext.TraceIdentifier));
        }
        return Ok(ApiResponseFactory.SuccessResponse(isSuccess, "Logout Successfully!", HttpContext.TraceIdentifier));
    }
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] Request.RefreshTokenRequest request)
    {
        var result = await _identityService.RefreshToken(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Token refreshed", HttpContext.TraceIdentifier));
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