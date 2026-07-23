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
    [HttpPost("generate-weekly")]
    public async Task<IActionResult> GenerateWeekly()
    {
        await _leaderboardService.GenerateWeeklyLeaderboard();
        return Ok(ApiResponseFactory.SuccessResponse(null, "Weekly leaderboard generated.", HttpContext.TraceIdentifier));
    }

    [Authorize]
    [HttpPost("generate-monthly")]
    public async Task<IActionResult> GenerateMonthly()
    {
        await _leaderboardService.GenerateMonthlyLeaderboard();
        return Ok(ApiResponseFactory.SuccessResponse(null, "Monthly leaderboard generated.", HttpContext.TraceIdentifier));
    }
}
