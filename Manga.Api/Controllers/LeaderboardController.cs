using Manga.Service.Leaderboard;
using Manga.Service.Model;
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

    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklyLeaderboard()
    {
        var result = await _leaderboardService.GetWeeklyLeaderboardAsync();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Weedkly Leaderboard Successfully", HttpContext.TraceIdentifier));
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyLeaderboard()
    {
        var result = await _leaderboardService.GetMonthlyLeaderboardAsync();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Monthly Leaderboard Successfully", HttpContext.TraceIdentifier));

    }
}
