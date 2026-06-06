using Manga.Api.extensions;
using Manga.Service.Model;
using Manga.Service.PublishingSchedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublishingScheduleController: ControllerBase
{
    private readonly IService _scheduleService;

    public PublishingScheduleController(IService scheduleService)
    {
        _scheduleService = scheduleService;
    }
    
    [Authorize(Policy = JwtExtensions.EditorialBoardPolicy)]
    [HttpPost("create-schedule")]
    public async Task<IActionResult> CreatePusblishingSchedule(Guid seriesId, [FromBody] Request.CreatePublishingScheduleRequest request)
    {
        var createSeries = await _scheduleService.CreatePublishingSchedule(seriesId, request);
        return Ok(ApiResponseFactory.SuccessResponse(createSeries, "Create Chapter Successfully", HttpContext.TraceIdentifier));
    }
}