using Manga.Api.extensions;
using Manga.Repository.Entity;
using Manga.Service.MangaTask;
using Manga.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class MangaTaskController : ControllerBase
{
    private readonly IService _mangaTaskservice;

    public MangaTaskController(IService mangaTaskService)
    {
        _mangaTaskservice = mangaTaskService;
    }

    [Authorize(Policy = JwtExtensions.MangakaPolicy)]
    [HttpPost("create-tasks")]
    public async Task<IActionResult> CreateNewTasks([FromBody] Request.CreateNewTaskRequest request)
    {
        var result = await _mangaTaskservice.CreateNewTask(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Create Task Successfully", HttpContext.TraceIdentifier));
    }
    // [Authorize(Policy = JwtExtensions.MangakaPolicy) ]
    // [Authorize(Policy = JwtExtensions.AssistantPolicy)]
    [HttpGet("get-tasks-details")]
    public async Task<IActionResult> GetTaskDetails([FromQuery] Request.GetTaskDetailsRequest request)
    {
        var result = await _mangaTaskservice.GetTaskDetails(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Task Details Successfully",
            HttpContext.TraceIdentifier));
    }
    [HttpGet("get-tasks-list")]
    public async Task<IActionResult> GetTaskList([FromQuery] Request.GetTaskListRequest request)
    {
        var result = await _mangaTaskservice.GetTaskList(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Task List Successfully",
            HttpContext.TraceIdentifier));
    }
}