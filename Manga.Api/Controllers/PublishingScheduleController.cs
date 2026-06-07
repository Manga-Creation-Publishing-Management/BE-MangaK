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
    
    [Authorize]
    [HttpGet("get-all-schedule")]
    public async Task<IActionResult> GetAllPublishingSchedule()
    {
        var result = await _scheduleService.GetAllPublishingSchedules();
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get All Publishing Schedule Successfully", HttpContext.TraceIdentifier));
    }

    [Authorize]
    [HttpPatch("update-schedule")]
    public async Task<IActionResult> UpdatePublishingSchedule(Guid scheduleId, [FromBody] Request.UpdatePublishingScheduleRequest request)
    {
        var result = await _scheduleService.UpdatePublishingSchedule(scheduleId, request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Update Publishing Schedule Successfully", HttpContext.TraceIdentifier));
    }

    [Authorize]
    [HttpDelete("delete-schedule")]
    public async Task<IActionResult> DeletePublishingSchedule(Guid scheduleId)
    {
         await _scheduleService.DeletePublishingSchedule(scheduleId);
        return Ok(ApiResponseFactory.SuccessResponse(null, "Delete Publishing Schedule Successfully", HttpContext.TraceIdentifier));
    }
    
}