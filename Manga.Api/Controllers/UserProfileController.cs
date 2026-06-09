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
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get user list Successfully", HttpContext.TraceIdentifier));
    }
}