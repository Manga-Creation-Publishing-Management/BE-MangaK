using Manga.Api.extensions;
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
   
    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    [HttpPost("generate-weekly")]
    public async Task<IActionResult> GenerateWeekly()
    {
        await _leaderboardService.GenerateWeeklyLeaderboard();
        return Ok(ApiResponseFactory.SuccessResponse(null, "Weekly leaderboard generated.", HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = JwtExtensions.AdminPolicy)]
    [HttpPost("generate-monthly")]
    public async Task<IActionResult> GenerateMonthly()
    {
        await _leaderboardService.GenerateMonthlyLeaderboard();
        return Ok(ApiResponseFactory.SuccessResponse(null, "Monthly leaderboard generated.", HttpContext.TraceIdentifier));
    }

    [HttpGet("periods")]
    public async Task<IActionResult> GetPeriods([FromQuery] string type)
    {
        var result = await _leaderboardService.GetAvailablePeriods(type);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Periods retrieved successfully.", HttpContext.TraceIdentifier));
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeekly([FromQuery] string? period)
    {
        var result = await _leaderboardService.GetWeeklyLeaderboard(period);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Weekly leaderboard retrieved successfully.", HttpContext.TraceIdentifier));
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly([FromQuery] string? period)
    {
        var result = await _leaderboardService.GetMonthlyLeaderboard(period);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Monthly leaderboard retrieved successfully.", HttpContext.TraceIdentifier));
    }
}
