using Manga.Api.extensions;
using Manga.Service.Model;
using Manga.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    private readonly IService _userProfileService;

    public UserProfileController(IService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet("get-user-list-by-role")]
    public async Task<IActionResult> GetUserListByRole([FromQuery]Request.GetUserListByRoleRequest request)
    {
        var result = await _userProfileService.GetUserListByRole(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get user list by role Successfully", HttpContext.TraceIdentifier));
    }
    
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    [HttpGet("get-user-list")]
    public async Task<IActionResult> GetUserList()
    {
        var result = await _userProfileService.GetUserList();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get user list Successfully", HttpContext.TraceIdentifier));
    }
    [Authorize]
    [HttpGet("get-profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userProfileService.GetProfile();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Profile Successfully", HttpContext.TraceIdentifier));
    }
    [Authorize]
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromForm]Request.UpdateProfileRequest request)
    {
        var result = await _userProfileService.UpdateProfile(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Update Profile Successfully", HttpContext.TraceIdentifier));
    }
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    [HttpPut("update-user-status")]
    public async Task<IActionResult> UpdateUserStatus([FromQuery] Request.UpdateUserStatusRequest request)
    {
        var result = await _userProfileService.UpdateUserStatus(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Update Status Successfully", HttpContext.TraceIdentifier));
    }
    [Authorize(Policy = JwtExtensions.MangakaPolicy)]
    [HttpGet("filter-assistant")]
    public async Task<IActionResult> FilterAssistant([FromQuery] Request.FilterAssistantRequest request)
    {
        var result = await _userProfileService.FilterAssistant(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Filter assistant Successfully", HttpContext.TraceIdentifier));
    }
}