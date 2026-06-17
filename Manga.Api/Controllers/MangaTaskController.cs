using Manga.Api.extensions;
using Manga.Repository.Entity;
using Manga.Service.MangaTask;
using Manga.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [Authorize]
    [HttpGet("get-tasks-details")]
    public async Task<IActionResult> GetTaskDetails([FromQuery] Request.GetTaskDetailsRequest request)
    {
        var result = await _mangaTaskservice.GetTaskDetails(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Task Details Successfully",
            HttpContext.TraceIdentifier));
    }
    [Authorize]
    [HttpGet("get-tasks-list")]
    public async Task<IActionResult> GetTaskList([FromQuery] Request.GetTaskListRequest request)
    {
        var result = await _mangaTaskservice.GetTaskList(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Get Task List Successfully",
            HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = JwtExtensions.AssistantPolicy)]
    [HttpPut("update-task-status")]
    public async Task<IActionResult> UpdateTaskStatus([FromBody] Request.UpdateTaskStatusRequest request)
    {
        var result = await _mangaTaskservice.UpdateTaskStatus(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Task Accepted Successfully", HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = JwtExtensions.AssistantPolicy)]
    [HttpPut("submit-task")]
    public async Task<IActionResult> SubmitTask([FromForm] Request.SubmitTaskRequest request)
    {
        var result = await _mangaTaskservice.SubmitTask(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Task Submitted Successfully", HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = JwtExtensions.MangakaPolicy)]
    [HttpPut("review-task")]
    public async Task<IActionResult> ReviewTask([FromBody] Request.ReviewTaskRequest request)
    {
        var result = await _mangaTaskservice.ReviewTask(request);
        return Ok(ApiResponseFactory.SuccessResponse(result, "Task Reviewed Successfully", HttpContext.TraceIdentifier));
    }
}