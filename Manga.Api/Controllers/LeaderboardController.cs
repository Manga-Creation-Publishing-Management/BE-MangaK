using Manga.Service.Leaderboard;
using Manga.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly IService _leaderboardService;

    public LeaderboardController(IService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }
    [Authorize]
    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklyLeaderboard()
    {
        var result = await _leaderboardService.GetWeeklyLeaderboard();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Weedkly Leaderboard Successfully", HttpContext.TraceIdentifier));
    }
    [Authorize]
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyLeaderboard()
    {
        var result = await _leaderboardService.GetMonthlyLeaderboard();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Monthly Leaderboard Successfully", HttpContext.TraceIdentifier));

    }
}
